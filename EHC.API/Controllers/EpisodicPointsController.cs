using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.API.Swagger.ResponseBodyExampleProviders;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Api;

namespace TLM.EHC.API.Controllers
{
    public class EpisodicPointsController : EquipmentController
    {
        private readonly IDataParser _dataParser;
        private readonly IDataMapper _dataMapper;
        private readonly IEpisodeService _episodeService;
        private readonly IApiImplementation _apiImplementation;

        public EpisodicPointsController(
            ITimestampParser timestampParser,
            IDataParser dataParser,
            IDataMapper dataMapper,
            IEpisodeService episodeService,
            IApiImplementation apiImplementation) : base(timestampParser)
        {
            _dataParser = dataParser;
            _dataMapper = dataMapper;
            _episodeService = episodeService;
            _apiImplementation = apiImplementation;
        }


        /// <summary>
        /// Get episodic points for multiple codes of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="episodeId">Specify EpisodeId.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="codes">Specify the codes.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(MultipleChannels))]
        [ProducesNotFoundResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesBadRequestResponseType]
        [HttpGet("episodic-points")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider),404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]

        public async Task<IActionResult> GetEpisodicPoints(string equipmentWkeId, string episodeId, string start, string end, string codes)
        {
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    EpisodeId = episodeId,
                    DataType = DataType.Episodic,
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
        /// Get episodic points for single code of single equipment
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="episodeId">Specify EpisodeId.</param>
        /// <param name="start">Time period start (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="end">Time period end (accepts multiple formats). Latest 24h period with data by default.</param>
        /// <param name="channelCode">Specify the channel codes.</param>
        /// <returns></returns>
        [ProducesOkResponseType(typeof(SingleChannel))]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [HttpGet("episodic-points/{channelCode}")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider),404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(ChannelCodeNotFoundExampleProvider),404)]

        public async Task<IActionResult> GetEpisodicPointsByCode(string equipmentWkeId, string episodeId, string start, string end, string channelCode)
        {
            try
            {
                var rowsRequest = new RowsRequest()
                {
                    EpisodeId = episodeId,
                    DataType = DataType.Episodic,
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
        /// Posting episodic points for multiple codes of single equipment within specific episode
        /// </summary>
        /// <param name="equipmentWkeId">Well-known entity id of equipment. Example: 101053903:280</param>
        /// <param name="channelData">Specify channel data in json object.See Example.</param>
        /// <returns></returns>
        [ProducesNotFoundResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ProducesBadRequestResponseType]
        [ProducesCreatedResponseType]
        [ConsumesApplicationJson]
        [HttpPost("episodic-points")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> PostEpisodicPoints(string equipmentWkeId, [FromBody] EpisodicPointsRequest channelData)
        {
            try
            {
                if (channelData == null) throw new BadRequestException("Invalid json");
                var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                var channels= JToken.FromObject(channelData.Meta.Channels);
                var parsedChannels = _dataParser.ParseChannelsData(channels);
                var mappedChannels = await _dataMapper.ValidateAndMapChannels(parsedChannels);
                string episodeId = channelData.Meta.EpisodeId;

                if (episodeId == null)
                {
                    throw new BadRequestException("EpisodeId should be specified.");
                }

                var episode = await _episodeService.GetEpisodeById(episodeId);
                var rows = JArray.FromObject(channelData.Rows);
                var parsedRows = _dataParser.ParseRowsData(rows);
                var influxRows = _dataMapper.MapToInfluxRows(parsedRows, mappedChannels);
                await _apiImplementation.SaveRows(wellKnownEqId, influxRows, DataType.Episodic, episodeId);
                if (episode != null)
                {
                    if (!episode.EquipmentWkeIdList.Contains(equipmentWkeId))
                    {
                        episode.EquipmentWkeIdList.Add(equipmentWkeId);
                        await _episodeService.UpdateEpisode(episode);
                    }
                }

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
        [HttpGet("episodic-point-definitions")]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForEquipmentIDExampleProvider), 400)]
        public async Task<IActionResult> GetEpisodicPointsDefinitions(string equipmentWkeId)
        {
            try
            {
                var wellKnownEqId = ParseWellKnownEqId(equipmentWkeId);
                var channels = await _apiImplementation.GetChannelDefinitions(wellKnownEqId, DataType.Episodic);
                return Ok(channels);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

    }
}
