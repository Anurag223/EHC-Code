using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Api;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.API.Swagger.ResponseBodyExampleProviders;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;


namespace TLM.EHC.API.Controllers
{
    public class ReadingsController : EquipmentController
    {
        private readonly IApiImplementation _apiImplementation;
        private readonly IDataParser _dataParser;
        private readonly IDataMapper _dataMapper;
        private readonly IHistorianClient _historianClient;

        public ReadingsController(
            IApiImplementation apiImplementation, 
            IDataParser dataParser,
            IDataMapper dataMapper,
            ITimestampParser timestampParser,
            IHistorianClient historianClient) : base(timestampParser)
        {
            _apiImplementation = apiImplementation;
            _dataParser = dataParser;
            _dataMapper = dataMapper;
            _historianClient = historianClient;
        }

        /// <summary>
        /// Get readings for multiple codes of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="codes">Specify the codes.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(MultipleChannels))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [HttpGet("readings")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]

        public async Task<IActionResult> GetReadings(string equipmentWkeId, string start, string end, string codes)
        {
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    DataType = DataType.Reading,
                    QueryType = QueryType.MultipleCodes,
                    WKEid = ParseWellKnownEqId(equipmentWkeId),
                    Codes = ParseCodes(codes),
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
        /// Get readings for single code of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="channelCode">Specify Channel code.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(SingleChannel))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [HttpGet("readings/{channelCode}")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(ChannelCodeNotFoundExampleProvider), 404)]

        public async Task<IActionResult> GetReadingByCode(string equipmentWkeId, string channelCode, string start, string end)
        {
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    DataType = DataType.Reading,
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
        /// Posting readings for multiple codes of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="channelData">Specify channel data in json object.See Example.</param>
        /// <returns></returns>
        [ProducesNotFoundResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesBadRequestResponseType]
        [ProducesCreatedResponseType]
        [ConsumesApplicationJson]
        [HttpPost("readings")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> PostReadings(string equipmentWkeId, [FromBody] MultipleChannelsRequest channelData)
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
                await _apiImplementation.SaveRows(wellKnownEqId, influxRows, DataType.Reading);
                return Ok();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Get all channel codes with at least one data point for given equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(ChannelDefinitionClean[]))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [HttpGet("reading-definitions")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> GetReadingDefinitions(string equipmentWkeId)
        {
            try
            {
                var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                var channels = await _apiImplementation.GetChannelDefinitions(wellKnownEqId, DataType.Reading);
                return Ok(channels);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }




        [HttpGet("test-influx-db-fri")]
       // public async Task<IActionResult> TestInfluxDb()
        private async Task<IActionResult> TestInfluxDb()
        {
            try
            {
                var result = await _historianClient.ShowDatabases();
                string text = string.Join(Environment.NewLine, result.Values.SelectMany(x => x));
                return Content(text);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }
    }
}
