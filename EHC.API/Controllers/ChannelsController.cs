#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.API.Swagger.RequestBodyExampleProvider;
using TLM.EHC.API.WritingData;
using Tlm.Sdk.Api;
using TLM.EHC.API.Swagger.ResponseBodyExampleProviders;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.API.Models;

namespace TLM.EHC.API.Controllers
{
    public class ChannelsController : EquipmentController
    {
        private readonly IApiImplementation _apiImplementation;
        private readonly IDataParser _dataParser;
        private readonly IDataMapper _dataMapper;

        public ChannelsController(
            IApiImplementation apiImplementation,
            IDataParser dataParser,
            IDataMapper dataMapper,
            ITimestampParser timestampParser
        ) : base(timestampParser)
        {
            _apiImplementation = apiImplementation;
            _dataParser = dataParser;
            _dataMapper = dataMapper;
        }

        /// <summary>
        /// Get raw channel data for multiple codes of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="codes">Comma-separated channel codes. Returns all possible codes by default.</param>
        /// <param name="aggregationFunction">This needs to be selected when groupby value is defined</param>
        /// <param name="groupbyTimeValue">This will be used when aggregationFunction is selected.The format of the values should be 10h/1m/20s etc</param>
        /// <param name="fillValue">This will be set to null by default if groupbyTimeValue is used.The possible values are none/previous/linear and any number as 1/2/3 etc</param>
        /// <response code="200">Raw channel data for the multiple codes of single equipment</response>
        /// <response code="400">Invalid valid values in request</response>
        /// <response code="404">Oops! Can't get the channels</response>
        [ProducesOkResponseType(typeof(MultipleChannels))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("channels")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]

        public async Task<IActionResult> GetChannels(string equipmentWkeId, string start, string end, string codes, AggregationFunctions? aggregationFunction = null, string? groupbyTimeValue = null,string? fillValue=null)
        {
            
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    DataType = DataType.Channel,
                    QueryType = QueryType.MultipleCodes,
                    WKEid = ParseWellKnownEqId(equipmentWkeId),
                    Codes = ParseCodes(codes),
                    TimePeriod = ParseTimePeriod(start, end),
                    ResponseFormat = ParseResponseFormatFromHeader(),
                    AggregateFunction = (aggregationFunction!= null && aggregationFunction != AggregationFunctions.None)? aggregationFunction:null,
                    GroupbyTimeValue = groupbyTimeValue,
                    FillValue = fillValue
                };               
               
                var response = await _apiImplementation.GetRows(rowsRequest);
               
                return ConvertToActionResult(response);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Gets tha calculated channels data using mathematical operations.
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="firstChannelCode">First operand as Channel code in which operation needs to be performed.</param>
        /// <param name="secondChannelCode">Second operand as Channel code in which operation needs to be performed.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="fillValue">This will be set to null by default.The possible values are none/previous/linear and any number as 1/2/3 etc</param>
        /// <param name="mathFunction">By default Add operation will selected.</param>
        /// <response code="200">Calculated channel data of single equipment</response>
        /// <response code="400">Invalid valid values in request</response>
        /// <response code="404">Oops! Can't get the channels</response>
        [ProducesOkResponseType(typeof(MultipleChannels))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]     
        [HttpGet("calculatedchannels/{firstChannelCode}/{secondChannelCode}")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForInvalidFillValueExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(ChannelCodeNotFoundExampleProvider), 404)]
        public async Task<IActionResult> GetCalculatedChannels(string equipmentWkeId, string firstChannelCode, string secondChannelCode, string start, string end,string fillValue, MathFunctions mathFunction = MathFunctions.Add)
        {

            try
            {  
                var rowRequest = new RowsRequest()
                {
                    DataType = DataType.Channel,
                    QueryType = QueryType.MultipleCodes,
                    WKEid = ParseWellKnownEqId(equipmentWkeId),
                    TimePeriod = ParseTimePeriod(start, end),
                    ResponseFormat = ParseResponseFormatFromHeader(),
                    MathFunction = mathFunction,
                    Codes=new[] { firstChannelCode, secondChannelCode },                   
                    FillValue = fillValue
                };

                var response = await _apiImplementation.GetCalculatedRows(rowRequest);
                return Ok(response);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get raw channel data for single code of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment.</param>
        /// <param name="channelCode">Channel code.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data if empty.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data if empty.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(SingleChannel))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("channels/{channelCode}")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(ChannelCodeNotFoundExampleProvider), 404)]

        public async Task<IActionResult> GetChannelByCode(string equipmentWkeId, string channelCode, string start, string end)
        {
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    DataType = DataType.Channel,
                    QueryType = QueryType.SingleCode,
                    WKEid = ParseWellKnownEqId(equipmentWkeId),
                    Codes = new[] { channelCode },
                    TimePeriod = ParseTimePeriod(start, end),
                    ResponseFormat = ParseResponseFormatFromHeader()
                };

                var response = await _apiImplementation.GetRows(rowsRequest);
                return ConvertToActionResult(response);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get latest channel timestamp that was recorded prior to the Threshold TimeStamp, for specified equipment WkeId
        /// </summary>
        /// <param name="equipmentWkeId">Well known entity id of an equipment</param>
        /// <param name="channelCode">Channel code</param>
        /// <param name="thresholdTimeStamp"> Time stamp date provided by user</param>
        /// <returns>Latest Timestamp value for a provided Equipment Wkeid and channel code</returns>
        [HttpGet("latesttimestamp/{channelCode}")]
        [ProducesOkResponseType(typeof(TimestampChannelData))]
        [ProducesBadRequestResponseType(Type = typeof(TimestampChannelData))]
        [ProducesNotFoundResponseType(Type =typeof(TimestampChannelData))]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(ChannelCodeNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(InvalidUserTimestampExampleProvider),400)]
        public async Task<IActionResult> GetLatestTimeStampForChannels(string equipmentWkeId, string channelCode, string thresholdTimeStamp)
        {
            try
            {
                var isValidFormat = IsValidZuluTimePeriod(thresholdTimeStamp);
                if (isValidFormat)
                {
                    if (!string.IsNullOrEmpty(equipmentWkeId))
                    {
                        if (!string.IsNullOrEmpty(channelCode))
                        {
                            var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                            List<TimestampChannelData> result =
                                await _apiImplementation.GetChannelLatestTimeStampWithCode(wellKnownEqId, equipmentWkeId,
                                    channelCode, thresholdTimeStamp);
                            return Ok(result);
                        }

                        throw new BadRequestException("Channel code cannot be null");
                    }

                    throw new BadRequestException("Equipment wkeid cannot be null");
                }
                else
                {
                    throw new BadRequestException(
                        "Invalid Date format.Please provide date in the format of eg- 2019-07-29T15:30:04.431Z");
                }
                
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }
        /// <summary>
        /// Posting raw channel data for multiple codes of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment.</param>
        /// <param name="channelData">JSON object. See example.</param>
        /// <returns></returns>

        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesCreatedResponseType]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.InternalServerError)]
        [ConsumesApplicationJson]
        [HttpPost("channels")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> PostChannels(string equipmentWkeId, [FromBody] MultipleChannelsRequest channelData)
        {
            try
            {
                if (channelData == null) throw new BadRequestException("Invalid json");
                var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                var channels = JToken.FromObject(channelData.Meta.Channels);
                var parsedChannels = _dataParser.ParseChannelsData(channels);
                var mappedChannels = await _dataMapper.ValidateAndMapChannels(parsedChannels);
                var rows = JArray.FromObject(channelData.Rows);
                var parsedRows = _dataParser.ParseRowsData(rows);
                var influxRows = _dataMapper.MapToInfluxRows(parsedRows, mappedChannels);
                await _apiImplementation.SaveRows(wellKnownEqId, influxRows, DataType.Channel);
                return new OkResult();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get all channel codes with at least one data point for given equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(ChannelDefinitionClean[]))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [HttpGet("channel-definitions")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> GetChannelDefinitions(string equipmentWkeId)
        {
            try
            {
                var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                var channels = await _apiImplementation.GetChannelDefinitions(wellKnownEqId, DataType.Channel);
                return new OkObjectResult(channels);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Get raw channel data for multiple equipments (bulk get)
        /// </summary>
        /// <param name="equipmentWkeIdList">Comma-separated well-known entity id list.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data if empty.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data if empty.</param>
        /// <param name="codes">Comma-separated channel codes. Returns all possible codes if empty.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(BulkRows))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [HttpGet("/v2/channels")]
        public async Task<IActionResult> GetBulkChannels(string equipmentWkeIdList, string start, string end, string codes)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(equipmentWkeIdList))
                {
                    throw new BadRequestException("Comma separated wkeid list should be specified.");
                }

                WellKnownEntityId[] wkeidArray = equipmentWkeIdList
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ParseWellKnownEqId)
                    .ToArray();

                TimePeriod timePeriod = ParseTimePeriod(start, end);
                string[] codeArray = ParseCodes(codes);

                var bulkRows = await _apiImplementation.GetBulkRows(wkeidArray, timePeriod, codeArray, DataType.Channel);
                return new OkObjectResult(bulkRows);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Posting raw channel data for multiple equipments (bulk post)
        /// </summary>
        /// <param name="bulkChannelData">JSON object. See example.</param>
        /// <returns></returns>
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesCreatedResponseType]
        [ConsumesApplicationJson]
        [HttpPost("/v2/channels")]
        [RequestBodyExampleProviderReference(typeof(ChannelsPostBulkExampleProvider), Tlm.Sdk.Core.Models.Hypermedia.Constants.MediaType.ApplicationJson)]
        public async Task<IActionResult> PostBulkChannels([FromBody] JToken bulkChannelData)
        {
            try
            {
                if (bulkChannelData == null) throw new BadRequestException("Invalid json");
                if (bulkChannelData["equipmentWkeMappings"] != null)
                {
                    // see SendBulkWithMappings.json
                    bulkChannelData = _dataParser.ConvertEquipmentMappingsToStandardBulk(bulkChannelData);
                }
                var parsedChannels = _dataParser.ParseChannels(bulkChannelData);
                var mappedChannels = await _dataMapper.ValidateAndMapChannels(parsedChannels);
                var parsedRowsList = _dataParser.ParseRowsBulk(bulkChannelData);
                var bulk = _dataMapper.MapToInfluxRowsBulk(parsedRowsList, mappedChannels);
                await _apiImplementation.SaveRowsBulk(bulk, DataType.Channel);
                return Ok();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

    }
}
