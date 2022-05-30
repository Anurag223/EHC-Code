using MoreLinq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using EHC.Common.Models;
using Serilog;
using TLM.EHC.Admin;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.HyperLinks;
using TLM.EHC.API.Models;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using Vibrant.InfluxDB.Client.Rows;
using ILogger = Serilog.ILogger;

namespace TLM.EHC.API.Controllers
{
    public interface IApiImplementation
    {
        Task<ApiResponse> GetRows(RowsRequest rowsRequest);
        Task<EpisodeRows> GetEpisodeRows(string episodeId, TimePeriod timePeriod, DataType dataType);
        Task<BulkRows> GetBulkRows(WellKnownEntityId[] wkeidArray, TimePeriod timePeriod, string[] codes, DataType dataType);

        Task SaveRows(WellKnownEntityId wkeid, DynamicInfluxRow[] influxRows, DataType dataType, string episodeId = null);
        Task SaveRowsBulk((WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[] rowsArray, DataType dataType, string episodeId = null);

        Task<ChannelDefinitionClean[]> GetChannelDefinitions(WellKnownEntityId wkeid, DataType dataType);
        Task <List<TimestampChannelData>> GetChannelLatestTimeStampWithCode(WellKnownEntityId wellKnownEqId,string equipmentWkeId, string channelCode, string userProvidedDate);
        Task<ApiResponse> GetCalculatedRows(RowsRequest rowsRequest);

    }


    public class ApiImplementation : IApiImplementation
    {
        private readonly IEquipmentProvider _equipmentProvider;
        private readonly IEpicV3HierarchyProvider _epicV3HierarchyProvider;
        private readonly IResponseProviderResolver _responseProviderResolver;
        private readonly IHistorianClient _historianClient;
        private readonly IHyperLinksProvider _hyperLinksProvider;
        private readonly IHistorianWriter _historianWriter;
        private readonly IEpisodeService _episodeService;
        private readonly IChannelDefinitionService _channelDefinitionService;
        private readonly IInfluxDBMappingService _influxDbMappingService;
        private readonly IUrlBuilder _urlBuilder;
        private readonly ITimestampParser _timestampParser;
        private readonly EhcApiConfig _apiConfig;
        private static readonly ILogger Logger = Log.Logger.ForContext<ApiImplementation>();


        public ApiImplementation(
            IEquipmentProvider equipmentProvider,
            IEpicV3HierarchyProvider epicV3HierarchyProvider,
            IResponseProviderResolver responseProviderResolver,
            IHistorianClient historianClient,
            IHyperLinksProvider hyperLinksProvider,
            IHistorianWriter historianWriter,
            IEpisodeService episodeService,
            IChannelDefinitionService channelDefinitionService,
            EhcApiConfig apiConfig,
        IInfluxDBMappingService influxDbMappingService,IUrlBuilder builder,ITimestampParser timestampParser
        )
        {
            _equipmentProvider = equipmentProvider;
            _epicV3HierarchyProvider = epicV3HierarchyProvider;
            _responseProviderResolver = responseProviderResolver;
            _historianClient = historianClient;
            _hyperLinksProvider = hyperLinksProvider;
            _historianWriter = historianWriter;
            _episodeService = episodeService;
            _channelDefinitionService = channelDefinitionService;
            _influxDbMappingService = influxDbMappingService;
            _urlBuilder = builder;
            _timestampParser = timestampParser;
            _apiConfig = apiConfig;
        }


        public async Task<ApiResponse> GetRows(RowsRequest rowsRequest)
        {
            Equipment equipment = await GetEquipment(rowsRequest.WKEid);  //Here we will get Equipment with classifications
            var influxPath = await GetInfluxPath(equipment);
            if (rowsRequest.DataType == DataType.Episodic && !string.IsNullOrEmpty(rowsRequest.EpisodeId))
            {
                await GetEpisode(rowsRequest.EpisodeId); // check if episode exists
            }
            var responseProvider = _responseProviderResolver.GetResponseProvider(rowsRequest.ResponseFormat, rowsRequest.QueryType);
            var (query, timePeriod) = await BuildQuery(equipment.EquipmentWkeId, influxPath, rowsRequest);

            var apiResponse = await GetApiResponse(rowsRequest, equipment, query, responseProvider, timePeriod);

            return apiResponse;
        }

        /// <summary>
        /// This method creates query with MathFunctions and create a default time period and queries influx client
        /// </summary>
        /// <param name="rowsRequest"></param>
        /// <returns></returns>
        public async Task<ApiResponse> GetCalculatedRows(RowsRequest rowsRequest)
        {
            await _channelDefinitionService.ValidateChannelCode(rowsRequest.Codes.ToList());
            ValidateFillValue(rowsRequest.FillValue);
            Equipment equipment = await GetEquipment(rowsRequest.WKEid);  
            var influxPath = await GetInfluxPath(equipment);
            var responseProvider = _responseProviderResolver.GetResponseProvider(rowsRequest.ResponseFormat, rowsRequest.QueryType);
            var (query, timePeriod) = await BuildCalculatedChannelsQuery(equipment.EquipmentWkeId, influxPath, rowsRequest);

            var apiResponse = await GetApiResponse(rowsRequest, equipment, query, responseProvider, timePeriod);

            return apiResponse;
        }

        private static void ValidateFillValue(string rowsRequestFillValue)
        {
            if (rowsRequestFillValue == null) return;
            var isValidFillValue=Enum.TryParse(rowsRequestFillValue, true,out FillValues _);
            if (isValidFillValue) return;
            bool isValidNumber = int.TryParse(rowsRequestFillValue, out _);
            if (!isValidFillValue || !isValidNumber)
            {
                throw new BadRequestException(EhcConstants.InvalidFillValue){ErrorCode = ErrorCodes.InvalidFillValue};
            }
        }

        public async Task<BulkRows> GetBulkRows(WellKnownEntityId[] wkeidArray, TimePeriod timePeriod, string[] codes, DataType dataType)
        {
            var result = new BulkRows();
            result.EquipmentList = new List<MultipleChannels>();

            var equipmentList = new List<Equipment>();

            foreach (WellKnownEntityId wkeid in wkeidArray)
            {
                var equipment = await _equipmentProvider.GetEquipmentByWkeid(wkeid);

                if (equipment == null)
                {
                    throw new NotFoundException(EhcConstants.EquipmentCannotBeFound + wkeid) { ErrorCode = ErrorCodes.EquipmentNotFound };
                }

                equipmentList.Add(equipment);
            }


            foreach (var equipment in equipmentList)
            {
                const string episodeId = null;
                var multipleChannels = await GetMultipleChannelsResponse(equipment, timePeriod, dataType, episodeId, codes);
                result.EquipmentList.Add(multipleChannels);
            }

            return result;
        }


        public async Task<EpisodeRows> GetEpisodeRows(string episodeId, TimePeriod timePeriod, DataType dataType)
        {
            EpisodeRows result = new EpisodeRows();
            result.EpisodeId = episodeId;
            result.EquipmentList = new List<MultipleChannels>();

            Episode episode = await _episodeService.GetEpisodeById(episodeId);

            if (dataType == DataType.Channel)
            {
                timePeriod = new TimePeriod(episode.StartTime, episode.EndTime);
            }

            var equipmentList = new List<Equipment>();

            foreach (string equipmentWkeId in episode.EquipmentWkeIdList)
            {
                var equipment = await _equipmentProvider.GetEquipmentByWkeid(WellKnownEntityId.Parse(equipmentWkeId));

                if (equipment == null)
                {
                    throw new NotFoundException(EhcConstants.EquipmentCannotBeFound + equipmentWkeId) { ErrorCode = ErrorCodes.EquipmentNotFound };
                }

                equipmentList.Add(equipment);
            }

            foreach (var equipment in equipmentList)
            {
                string[] codes = new string[0];
                var multipleChannels = await GetMultipleChannelsResponse(equipment, timePeriod, dataType, episodeId, codes);
                result.EquipmentList.Add(multipleChannels);
            }

            return result;
        }

        private async Task<ApiResponse> GetApiResponse(RowsRequest rowsRequest, Equipment equipment, Query query,
            ResponseProvider responseProvider, TimePeriod timePeriod)
        {
            ApiResponse apiResponse;
            var queryContext = new QueryContext { Equipment = equipment };

            if (query == null)
            {
                // no time period passed and no any data found to set default period of latest 24h
                apiResponse = await responseProvider.GetResponseNoData(rowsRequest, queryContext);
            }
            else
            {
                apiResponse = await responseProvider.GetResponse(query, queryContext);
                Logger.Information(query.SelectText);
            }

            if (apiResponse.Entity != null)
            {
                apiResponse.Entity.Links = _hyperLinksProvider.GetHyperLinks(rowsRequest, timePeriod);
            }

            return apiResponse;
        }

        private async Task<MultipleChannels> GetMultipleChannelsResponse(Equipment equipment, TimePeriod timePeriod, DataType dataType, string episodeId, string[] codes)
        {
            var influxPath = await GetInfluxPath(equipment);

            RowsRequest rowsRequest = new RowsRequest();
            rowsRequest.DataType = dataType;
            rowsRequest.QueryType = QueryType.MultipleCodes;
            rowsRequest.Codes = codes;
            rowsRequest.EpisodeId = episodeId;
            rowsRequest.TimePeriod = timePeriod;

            var (query, _) = await BuildQuery(equipment.EquipmentWkeId, influxPath, rowsRequest);
            var queryContext = new QueryContext { Equipment = equipment };
            ApiResponse apiResponse;
            var responseProvider = _responseProviderResolver.GetResponseProvider(ResponseFormat.V2, QueryType.MultipleCodes);

            if (query == null)
            {
                apiResponse = await responseProvider.GetResponseNoData(rowsRequest, queryContext);
            }
            else
            {
                apiResponse = await responseProvider.GetResponse(query, queryContext);
            }

            MultipleChannels multipleChannels = apiResponse.Entity as MultipleChannels;

            if (multipleChannels == null)
            {
                throw new BadRequestException(EhcConstants.NonNullMultipleChannelsExpected);
            }

            RemoveEpisodeChannel(multipleChannels);
            return multipleChannels;
        }

        private void RemoveEpisodeChannel(MultipleChannels multipleChannels)
        {
            if (multipleChannels.Meta?.Channels == null)
            {
                return;
            }

            const int index = 1;
            var channelEpisode = multipleChannels.Meta.Channels[index];

            if (channelEpisode.Code != "Episode")
            {
                return;
            }

            multipleChannels.Meta.Channels.RemoveAt(index);

            foreach (var row in multipleChannels.Rows)
            {
                row.RemoveAt(index);
            }
        }

        private async Task<(Query, TimePeriod)> BuildCalculatedChannelsQuery(string wkeid, InfluxPath influxPath, RowsRequest rowsRequest)
        {
            var queryBuilder = new QueryBuilder()
                    .UseTechnology(influxPath.Technology)
                    .UseBrand(influxPath.Brand)
                    .UseDataType(rowsRequest.DataType)
                    .FilterByWkeId(wkeid);
            
            // To create filter for the timeperiod based on the channel codes
            var queryBuilderForTimePeriod = queryBuilder.CopyObject<QueryBuilder>();       
            var fields = new List<string>
            {    await _channelDefinitionService.GetFieldName(rowsRequest.Codes[0]),
                 await _channelDefinitionService.GetFieldName(rowsRequest.Codes[1])
            };
            queryBuilderForTimePeriod.SelectFields(fields.ToArray());

            // To create Select statement with operators
            queryBuilder.SelectFieldsWithMathFunction(fields[0], fields[1], rowsRequest.MathFunction,rowsRequest.Codes[0],rowsRequest.Codes[1]);

            var timePeriod = await ResolveTimePeriod(rowsRequest, queryBuilderForTimePeriod);

            if (timePeriod == null)
            {
                return (null, null);
            }           
            queryBuilder.FilterByTimePeriod(timePeriod); 
            queryBuilder.UseFill(rowsRequest.FillValue);
            return (queryBuilder.GetQuery(), timePeriod);
        }


            private async Task<(Query, TimePeriod)> BuildQuery(string wkeid, InfluxPath influxPath, RowsRequest rowsRequest)
        {
            var queryBuilder = new QueryBuilder()
                .UseTechnology(influxPath.Technology)
                .UseBrand(influxPath.Brand)
                .UseDataType(rowsRequest.DataType)
                .FilterByWkeId(wkeid);
            // Creates a deep copy of the object so that it can be utilized for timeperiod query   
            var querybuilderForTimePeriod = queryBuilder.CopyObject<QueryBuilder>();

            switch (rowsRequest.QueryType)
            {
                case QueryType.SingleCode:
                    querybuilderForTimePeriod.SelectOneField(await _channelDefinitionService.GetFieldName(rowsRequest.Codes[0]));

                    if (rowsRequest.AggregateFunction != null)
                    {
                        queryBuilder.SelectOneFieldWithAggregateFunction(
                            await _channelDefinitionService.GetFieldName(rowsRequest.Codes[0]),
                            rowsRequest.AggregateFunction);
                    }
                    else
                    {
                        queryBuilder.SelectOneField(await _channelDefinitionService.GetFieldName(rowsRequest.Codes[0]));
                    }
                    
                    break;

                case QueryType.MultipleCodes:
                    if (rowsRequest.Codes.Length == 0)
                    {
                        querybuilderForTimePeriod.SelectAllFields();
                        if (rowsRequest.AggregateFunction != null)
                        {
                            queryBuilder.SelectAllFieldsWithAggregationFunction(rowsRequest.AggregateFunction);
                        }
                        else
                        {
                            queryBuilder.SelectAllFields();
                        }
                    }
                    else
                    {
                        var fields = new List<string>();

                        foreach (string code in rowsRequest.Codes)
                        {
                            fields.Add(await _channelDefinitionService.GetFieldName(code));
                        }
                        querybuilderForTimePeriod.SelectFields(fields.ToArray());
                        if (rowsRequest.AggregateFunction != null)
                        {
                            queryBuilder.SelectFieldsWithAggregationFunction(fields.ToArray(),
                                rowsRequest.AggregateFunction);
                        }
                        else
                        {
                            queryBuilder.SelectFields(fields.ToArray());
                        }
                        
                    }
                    break;

                default:
                    throw new ArgumentException(EhcConstants.UnexpectedQueryType + rowsRequest.QueryType);
            }


            if (rowsRequest.DataType == DataType.Episodic)
            {
                queryBuilder.ForceEpisodeField();
                querybuilderForTimePeriod.ForceEpisodeField();

                if (!string.IsNullOrEmpty(rowsRequest.EpisodeId))
                {
                    queryBuilder.FilterByEpisodeId(rowsRequest.EpisodeId);
                    querybuilderForTimePeriod.FilterByEpisodeId(rowsRequest.EpisodeId);
                }
            }

            var timePeriod = await ResolveTimePeriod(rowsRequest, querybuilderForTimePeriod);

            if (timePeriod == null)
            {
                return (null, null);
            }

            if (rowsRequest.DataType == DataType.Channel || rowsRequest.DataType == DataType.Reading
                                                         || (rowsRequest.DataType == DataType.Episodic && rowsRequest.TimePeriod != null))
            {
                queryBuilder.FilterByTimePeriod(timePeriod);
            }

            if (!string.IsNullOrEmpty(rowsRequest.GroupbyTimeValue))
            {
                queryBuilder.UseGroupBy(rowsRequest.GroupbyTimeValue).UseFill(rowsRequest.FillValue);
            }
            return (queryBuilder.GetQuery(), timePeriod);
        }


        private async Task<TimePeriod> ResolveTimePeriod(RowsRequest rowsRequest, QueryBuilder queryBuilder)
        {
            if (rowsRequest.TimePeriod != null)
            {
                return rowsRequest.TimePeriod;
            }

            // no time period passed, let's try to find latest data
            var timePeriod = await GetLatest24HTimePeriod(queryBuilder);

            if (timePeriod != null)
            {
                return timePeriod;
            }

            return null;
        }



        private async Task<TimePeriod> GetLatest24HTimePeriod(QueryBuilder queryBuilder)
        {
            var queryLatestTimestamp = queryBuilder.GetQueryForLatestTimestamp();
            DateTime? latestTimestamp = await _historianClient.GetLatestTimestamp(queryLatestTimestamp);

            if (latestTimestamp == null)
            {
                // no any data found at all
                return null;
            }

            return new TimePeriod(latestTimestamp.Value.AddDays(-1), latestTimestamp.Value);
        }



        private async Task<Equipment> GetEquipment(WellKnownEntityId wkeid)
        {
            Equipment equipment = await _equipmentProvider.GetEquipmentByWkeid(wkeid);

            if (equipment == null)
            {
                throw new NotFoundException(EhcConstants.EquipmentCannotBeFound + wkeid) { ErrorCode = ErrorCodes.EquipmentNotFound };
            }

            return equipment;
        }

        private async Task GetEpisode(string episodeId)
        {
            var episode = await _episodeService.GetEpisodeById(episodeId);
            if (episode == null)
            {
                throw new BadRequestException(String.Format(EhcConstants.EpisodeNotFound, episodeId));
            }
        }


        public async Task SaveRows(WellKnownEntityId wkeid, DynamicInfluxRow[] influxRows, DataType dataType, string episodeId = null)
        {
            if (!string.IsNullOrWhiteSpace(episodeId))
            {
                await GetEpisode(episodeId); // checking if episode exists
            }

            Equipment equipment = await GetEquipment(wkeid);

            foreach (var row in influxRows)
            {
                AddTags(row, equipment, episodeId);
            }

            string suffix = GetSuffix(dataType);

            var influxPath = await GetInfluxPath(equipment, "POST");

            await _historianWriter.WriteData(influxRows, influxPath, suffix);
        }


        public async Task SaveRowsBulk((WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[] rowsArray, DataType dataType, string episodeId = null)
        {
            if (!string.IsNullOrWhiteSpace(episodeId))
            {
                await GetEpisode(episodeId); // checking if episode exists
            }

            // before any data written we should find all paths via equipment models, may be some models can't be found

            var list = new List<(InfluxPath influxPath, Equipment equipment, DynamicInfluxRow[] rows)>();
            string suffix = GetSuffix(dataType);

            foreach (var item in rowsArray)
            {
                Equipment equipment = await GetEquipment(item.wkeid);

                var influxPath = await GetInfluxPath(equipment, "POST");
                list.Add((influxPath, equipment, item.rows));
            }

            foreach (var item in list)
            {
                foreach (var row in item.rows)
                {
                    AddTags(row, item.equipment, episodeId);
                }

                await _historianWriter.WriteData(item.rows, item.influxPath, suffix);
            }
        }


        private void AddTags(DynamicInfluxRow influxRow, Equipment equipment, string episodeId)
        {
            influxRow.Tags.Add("EquipmentInstance", equipment.EquipmentWkeId); // wkeid
            influxRow.Tags.Add("EquipmentCode", equipment.EquipmentCode);
            //influxRow.Tags.Add("EquipmentSerialNumber", equipment.SourceSystemRecordId); // was old
            influxRow.Tags.Add("EquipmentSerialNumber", equipment.SerialNumber); // for PBI 1142321

            if (!string.IsNullOrWhiteSpace(episodeId))
            {
                influxRow.Tags.Add("Episode", episodeId);
            }
        }


        private string GetSuffix(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Channel: // raw channel data
                    return "";

                case DataType.Reading:
                    return ".Reading";

                case DataType.Episodic:
                    return ".Episodic";
            }

            throw new ArgumentException("Unexpected data type: " + dataType);
        }


        public async Task PostBulkRows(ParsedBulkData parsedBulkData, DataType dataType)
        {
            var dicEquipments = new Dictionary<string, Equipment>();
            string suffix = GetSuffix(dataType);

            foreach (var equipmentRow in parsedBulkData.EquipmentRows)
            {
                dicEquipments.Add(equipmentRow.EquipmentWkeId.Value, await GetEquipment(equipmentRow.EquipmentWkeId));
            }

            foreach (var equipmentRow in parsedBulkData.EquipmentRows)
            {
                Equipment equipment = dicEquipments[equipmentRow.EquipmentWkeId.Value];

                foreach (var row in equipmentRow.Rows)
                {
                    AddTags(row, equipment, parsedBulkData.EpisodeId);
                }

                var influxPath = await GetInfluxPath(equipment, "POST");
                await _historianWriter.WriteData(equipmentRow.Rows, influxPath, suffix);
            }
        }

        /// <summary>
        /// Get channel definitions with at least one data point for given equipment
        /// </summary>
        /// <param name="wkeid"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public async Task<ChannelDefinitionClean[]> GetChannelDefinitions(WellKnownEntityId wkeid, DataType dataType)
        {
            Equipment equipment = await GetEquipment(wkeid);

            var influxPath = await GetInfluxPath(equipment);

            string[] allFields = await GetAllFields(influxPath, dataType);

            if (allFields == null || allFields.Length == 0)
            {
                return new ChannelDefinitionClean[0];
            }

            // Need to split this up when there are tons of fields to check. Otherwise, influx cant handle large request size
            int sliceSize = 50;
            var list = new List<ChannelDefinitionClean>();


            for (int fieldIndex = 0; fieldIndex < allFields.Length; fieldIndex += sliceSize)
            {
                string[] fieldSubset = allFields.Slice(fieldIndex, sliceSize).ToArray();


                InfluxResponse response = await GetAllFieldsValues(influxPath, dataType, fieldSubset, wkeid.Value);

                string[] fieldsWithValues = response.Results
                    .Select(x => x.Series)
                    .Where(x => x != null)
                    .Select(x => x.First().Columns[1])
                    .ToArray();



                foreach (string field in fieldsWithValues)
                {
                    list.Add(await _channelDefinitionService.GetChannelDescription(field));
                }

            }
            return list.ToArray();
        }



        private async Task<InfluxResponse> GetAllFieldsValues(InfluxPath influxPath, DataType dataType, string[] allFields, string wkeid)
        {
            var query = new QueryBuilder()
                .UseTechnology(influxPath.Technology)
                .UseBrand(influxPath.Brand)
                .UseDataType(dataType)
                .FilterByWkeId(wkeid)
                .SelectFields(allFields)
                .GetMultiQueryAllFieldsValues();

            var result = await _historianClient.PerformMultiQuery(query);
            return result;
        }


        private async Task<string[]> GetAllFields(InfluxPath influxPath, DataType dataType)
        {
            var query = new QueryBuilder()
                .UseTechnology(influxPath.Technology)
                .UseBrand(influxPath.Brand)
                .UseDataType(dataType)
                .GetQueryShowAllFields();

            var result = await _historianClient.PerformQuery(query);

            return result?.Values.Select(x => x.First() as string).ToArray();
        }

        private async Task<InfluxPath> GetInfluxPath(Equipment equipment, string httpType = "GET")
        {
            InfluxPath influxPath;
            if (equipment.EpicClassifications.Count < 1)
            {
                throw new ServerErrorException(EhcConstants.EquipmentClassificationNotFound);
            }

            EquipmentModel equipmentModel = new EquipmentModel();
            equipmentModel.EquipmentCode = equipment.EpicClassifications.Where(x => x.Type == EpicV3ClassificationType.EquipmentSystem.ToString()).Select(x => x.Code).FirstOrDefault();
            equipmentModel.MaterialNumber = equipment.MaterialNumber;

            InfluxDBMapping influxDb = await _influxDbMappingService.GetInfluxDBMapping(equipmentModel.EquipmentCode, true);
            if (influxDb != null)
            {
                influxPath = GetInfluxPathIfEquipmentCodeExistsInDbMap(httpType, equipmentModel, influxDb, equipment.EquipmentWkeId, _apiConfig.EhcSupportEmail);
            }
            else
            {
                influxPath = await GetInfluxPathIfEquipmentCodeDoNotExistInDbMap(equipment, httpType, equipmentModel);
            }
            return influxPath;
        }


        private static InfluxPath GetInfluxPathIfEquipmentCodeExistsInDbMap(string httpType, EquipmentModel equipmentModel, InfluxDBMapping influxDb, string equipmentWkeId, string supportEmail)
        {
            InfluxPath influxPath = InfluxPath.GetFromInfluxDBMapping(influxDb, equipmentModel.EquipmentCode);
            if (httpType == "POST" && influxDb.Status == InfluxDBStatus.Disabled)
            {
                throw new BadRequestException(string.Format(EhcConstants.InactiveDbMap,supportEmail,equipmentWkeId, equipmentModel.EquipmentCode));
            }
            return influxPath;
        }
        private async Task<InfluxPath> GetInfluxPathIfEquipmentCodeDoNotExistInDbMap(Equipment equipment, string httpType,  EquipmentModel equipmentModel)
        {
            equipmentModel.BrandCode = equipment.EpicClassifications.Where(x => x.Type == EpicV3ClassificationType.Brand.ToString()).Select(x => x.Code).FirstOrDefault();
            equipmentModel.TechnologyCode = equipment.EpicClassifications.Where(x => x.Type == EpicV3ClassificationType.Technology.ToString()).Select(x => x.Code).FirstOrDefault();

            if (equipmentModel.BrandCode == null || equipmentModel.TechnologyCode == null)
                return null;

            string techCode = Convert.ToString((int)EpicV3ClassificationType.Brand) + ":" + equipmentModel.TechnologyCode;

            // Plus we need to call Epic V3 hierarchy api to get Technology name and Brand name
            EpicV3Hierarchy hierarchy = await _epicV3HierarchyProvider.GetEpicHierarchyInfoFromCode(techCode);
            equipmentModel.TechnologyName = hierarchy.Name;
            equipmentModel.BrandName = hierarchy.Children.Where(x => x.Code == equipmentModel.BrandCode).Select(x => x.Name).FirstOrDefault();

            var influxPath = InfluxPath.GetFromEquipmentModel(equipmentModel);

            if (httpType == "POST")
            {
                InfluxMappingResponse res = await CreateDbMappingOrAddEquipmentCode(influxPath, equipmentModel);
                if (res == null)
                    throw new DataException(EhcConstants.DbMapCreationFailure);

                if (res.DBStatus == InfluxDBStatus.Enabled)
                {
                    if (!res.IsNewMeasurement)  //we don't want to create new measurement in influx if already old one exists in db mapping table
                    {
                        InfluxDBMapping map = new InfluxDBMapping();
                        map.MeasurementName = res.MeasurementName;
                        map.DbName = res.DBName;
                        map.EquipmentCodes = new List<string> { equipmentModel.EquipmentCode };
                        influxPath = InfluxPath.GetFromInfluxDBMapping(map, equipmentModel.EquipmentCode);
                    }
                }
                else
                {
                    throw new BadRequestException(string.Format(EhcConstants.InactiveDbMap, _apiConfig.EhcSupportEmail, equipment.EquipmentWkeId, equipmentModel.EquipmentCode));
                }
            }
            return influxPath;
        }


        private async Task<InfluxMappingResponse> CreateDbMappingOrAddEquipmentCode(InfluxPath influxPath, EquipmentModel equipmentModel)
        {
            //Insert the new mapping or add equipment code in Mongo DB lookup table also.
            InfluxDBMapping dbToCreate = new InfluxDBMapping();
            dbToCreate.BrandName = equipmentModel.BrandName;
            dbToCreate.TechnologyName = equipmentModel.TechnologyName;
            dbToCreate.BrandCode = equipmentModel.BrandCode;
            dbToCreate.TechnologyCode = equipmentModel.TechnologyCode;
            dbToCreate.DbName = influxPath.Technology;
            dbToCreate.MeasurementName = influxPath.Brand;
            dbToCreate.EquipmentCodes = new List<string> { influxPath.EquipmentCode };
            dbToCreate.Status = InfluxDBStatus.Disabled;

            return await _influxDbMappingService.CreateUpdateDBMapping(dbToCreate);
        }

        public async Task<List<TimestampChannelData>> GetChannelLatestTimeStampWithCode(WellKnownEntityId wellKnownEqId,string equipmentWkeId,string channelCode,string userProvidedDate)
        {
            var originalTimeStampByUser = userProvidedDate;
            userProvidedDate = _timestampParser.ConvertToInfluxTimeStampInMilliseconds(userProvidedDate);
            var equipmentInfluxInfo = await GetInfluxDbMappingInfo(wellKnownEqId);
            
            string actualDbName = equipmentInfluxInfo.DbName;
            string dbName = "\"" + actualDbName + "\"";
            string measurementName = "\"" + equipmentInfluxInfo.MeasurementName + "\"";
            
            string unitFetchUrl =
                _urlBuilder.GetLatestTimeStampThroughQueryUrl(actualDbName, dbName, measurementName, userProvidedDate);
            string channelCodewithUnit = await GetChannelInfoWithUnitOfMeasurement(unitFetchUrl, equipmentWkeId, channelCode);

            string url = _urlBuilder.GetLatestTimeStampForChannel(actualDbName, channelCodewithUnit, dbName,
                measurementName, userProvidedDate);
            var data = await ResolveInfluxResponseForChannel(url);
            var result = data.Results[0].Series?.Where(x => x.Tags.EquipmentInstance == equipmentWkeId)?.FirstOrDefault()?.Values[0][0];
            
            List<TimestampChannelData> timestampDataForChannels = new List<TimestampChannelData>();
            if (result != null)
            {
                var responseTime = DateTimeOffset.Parse(result.ToString()!);
                var zuluFormatTime = responseTime.ToString("s") + "Z";
                TimestampChannelData timestampChannelResponse = new TimestampChannelData();
                
                timestampChannelResponse.Code = channelCode;
                timestampChannelResponse.ThresholdTimestamp = originalTimeStampByUser ?? DateTime.Now.ToString("s") + "Z";
                timestampChannelResponse.LatestTimestamp = zuluFormatTime;
                
                timestampDataForChannels.Add(timestampChannelResponse);
            }
            else
            {
                throw new NotFoundException(EhcConstants.RecordUnavailableForThresholdValue) { ErrorCode = ErrorCodes.InvalidThresholdTimestamp };
            }
            return timestampDataForChannels;
        }

        #region Retrieving Equipment and InfluxDB Mapping data
        private async Task<InfluxDBMapping> GetInfluxDbMappingInfo(WellKnownEntityId wellKnownEqId)
        {
            var equipment = await _equipmentProvider.GetEquipmentByWkeid(wellKnownEqId);
            if (equipment == null)
            {
                throw new NotFoundException("Equipment not found: " + wellKnownEqId) { ErrorCode = ErrorCodes.EquipmentNotFound };
            }
            InfluxDBMapping influxDb = await _influxDbMappingService.GetInfluxDBMapping(equipment.EquipmentCode, true);
            if (influxDb == null)
            {
                throw new NotFoundException(EhcConstants.EquipmentMappingCannotBeFound);
            }
            return influxDb;
        }


        #endregion

        #region Obtain Channel Code Info with Unit of measurement
        private async Task<string> GetChannelInfoWithUnitOfMeasurement(string url, string actualWkeId, string channelCode)
        {
            var channelInfoResult = await ResolveInfluxResponseForChannel(url);
            if (channelInfoResult.Results[0].Series == null)
            {
                throw new NotFoundException(EhcConstants.RecordUnavailableForThresholdValue) {ErrorCode = ErrorCodes.InvalidThresholdTimestamp};
            }
            var channelInfoColumns = channelInfoResult.Results[0].Series?.Where(x => x.Tags.EquipmentInstance == actualWkeId).FirstOrDefault()?.Columns;
            if (channelInfoColumns == null)
            {
                throw new NotFoundException(EhcConstants.WkeIdNotFoundInInfluxDb + actualWkeId);
            }
            var channelCodeWithUnit = channelInfoColumns.Find(x => x.Contains(channelCode));
            if (string.IsNullOrEmpty(channelCodeWithUnit))
            {
                throw new NotFoundException(EhcConstants.ChannelCodeCannotBeFound);
            }

            return channelCodeWithUnit;
        }

        #endregion

        private async Task<InfluxResponse> ResolveInfluxResponseForChannel(string url)
        {
            return await _historianClient.PerformMultiQuery(url);
        }
        
    }
}
