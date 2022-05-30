using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Swagger.ResponseBodyExampleProviders;
using TLM.EHC.Common.Exceptions;
using Vibrant.InfluxDB.Client.Rows;
using Episode = TLM.EHC.API.ControllerModels.Separated.Episode;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Common.Services;
using TLM.EHC.Common.Models;
using TLM.EHC.Common;

namespace TLM.EHC.API.Controllers
{
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("v2/episodes")]
    [RootPolicy]
    public class EpisodesController : BaseController
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<EpisodesController>();

        private readonly IGetCollectionFromCacheStrategy<Episode> _multiResourceGetter;
        private readonly IGetFromCacheStrategy<Episode> _singleResourceGetter;
        private readonly IApiImplementation _apiImplementation;
        private readonly IDataParser _dataParser;
        private readonly IDataMapper _dataMapper;
        private readonly IEpisodeService _episodeService;
        private readonly IEpisodicPointService _episodicPointService;
        private readonly IEquipmentProvider _equipmentProvider;
        private readonly EhcApiConfig _config;

        public EpisodesController(
            TimestampParser timestampParser,
            IGetCollectionFromCacheStrategy<Episode> multiResourceGetter,
            IGetFromCacheStrategy<Episode> singleResourceGetter,
            IApiImplementation apiImplementation,
            IDataParser dataParser,
            IDataMapper dataMapper,
            IEpisodeService episodeService,
            IEpisodicPointService episodicPointService,
            IEquipmentProvider equipmentProvider,
            EhcApiConfig config
        ) : base(timestampParser)
        {
            _multiResourceGetter = multiResourceGetter;
            _singleResourceGetter = singleResourceGetter;
            _apiImplementation = apiImplementation;
            _dataParser = dataParser;
            _dataMapper = dataMapper;
            _episodeService = episodeService;
            _episodicPointService = episodicPointService;
            _equipmentProvider = equipmentProvider;
            _config = config;
        }

        /// <summary>
        /// Get all episodes
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns></returns>
        [HttpGet]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<Episode>))]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForPageSizeAndNumberExampleProvider), 400)]
        public async Task<IActionResult> GetEpisodes([QuerySpecBinder(typeof(Episode), Key = nameof(EpisodesController))] QuerySpec querySpec)
        {
            return await _multiResourceGetter.GetCollection(querySpec);
        }


        /// <summary>
        /// Get single episode by id
        /// </summary>
        /// <param name="id">Episode id.</param>
        /// <param name="querySpec">QuerySpec</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(HypermediaDocument<Episode>))]
        public async Task<IActionResult> GetEpisodeById(string id,[QuerySpecBinder(typeof(Episode), ExclusionPolicies = QueryExclusionPolicies.None,Key = nameof(EpisodesController))] QuerySpec querySpec)
        {
            querySpec = SpecBuilder.FromQuery<Episode>(querySpec ?? QuerySpec.ForEverything);
            return await _singleResourceGetter.GetSingleRepresentationById<EpisodesController>(id, querySpec);
        }

        /// <summary>
        /// Add new episode along with episodic points
        /// </summary>
        /// <param name="episode">Episode data in json format. See example.</param>
        /// <returns></returns>
        [HttpPost]
        [ConsumesApplicationJson]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(Episode))]
        [ProducesResponseType(typeof(Episode), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateEpisode([FromBody] EpisodeRequest episode)
        {
            try
            {
                if (episode == null) throw new BadRequestException("Invalid json");
                (WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[] bulk = null;

                if (episode.Data != null) // if contains data points
                {
                    var jsonChannels = JToken.FromObject(episode.Channels);
                    var parsedChannels = _dataParser.ParseChannelsData(jsonChannels);
                    var mappedChannels = await _dataMapper.ValidateAndMapChannels(parsedChannels);
                    var parsedRowsList = _dataParser.ParseRowsBulkData(JToken.FromObject(episode.Data));
                    bulk = _dataMapper.MapToInfluxRowsBulk(parsedRowsList, mappedChannels);

                    await ValidateCreateEpisode(episode, parsedRowsList);
                }

                if (IsAvatarChildEpisode(episode))
                {
                    ValidateAvatarChildEpisode(episode);
                    await _episodeService.DeleteOldChildEpisodes(episode);
                }

                string episodeId = await _episodeService.CreateEpisode(episode);
                Episode createdEpisode = await _episodeService.GetEpisodeById(episodeId);

                if (bulk != null)
                {
                    await _apiImplementation.SaveRowsBulk(bulk, DataType.Episodic, episodeId);
                }

                if (IsAvatarChildEpisode(episode) && bulk != null)
                {
                    try
                    {
                        await NotifyODM(episode);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Notify ODM - error ocurred.");
                        Logger.Error(ex, ex.Message);
                    }
                }

                return Ok(createdEpisode);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        private async Task ValidateCreateEpisode(Episode episode, ParsedRows[] parsedRowsList)
        {
            if (episode.EquipmentWkeIdList == null)
            {
                throw new BadRequestException("Array episode.EquipmentIds is null");
            }

            var episodeIds = episode.EquipmentWkeIdList;
            var dataIds = parsedRowsList.Select(x => x.EquipmentWkeId.Value).ToList();

            var undeclaredIds = dataIds.Except(episodeIds).ToList();

            if (undeclaredIds.Count > 0)
            {
                throw new BadRequestException("Episode should contain all equipment ids. Missed ids: " + string.Join(",", undeclaredIds));
            }

            if (episode.ParentId != null)
            {
                try
                {
                    var parent = await _episodeService.GetEpisodeById(episode.ParentId);
                }
                catch (NotFoundException)
                {
                    throw new BadRequestException("Parent episode not found: " + episode.ParentId);
                }

            }
        }


        /// <summary>
        /// Update episode
        /// </summary>
        /// <param name="id">Episode id.</param>
        /// <param name="episode">Episode data in json format. See example.</param>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesNoContentResponseType]
        [ConsumesApplicationJson]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        public async Task<IActionResult> UpdateEpisode(string id, [FromBody] EpisodeRequest episode)
        {
            try
            {
                if (episode == null) throw new BadRequestException("Invalid json");
                if (episode.Id != id)
                {
                    throw new BadRequestException("Mismatching episode Id.");
                }

                var oldEpisode = await _episodeService.GetEpisodeById(id);

                var equipmentIdsToRemove = oldEpisode.EquipmentWkeIdList.Except(episode.EquipmentWkeIdList).ToList();

                foreach (string equipmentId in equipmentIdsToRemove)
                {
                    if (await _episodicPointService.AnyPointsExists(episode.Id, equipmentId))
                    {
                        throw new BadRequestException("Can't un-reference episode with episodic points for " + equipmentId);
                    }
                }

                foreach (string equipmentWkeId in episode.EquipmentWkeIdList)
                {
                    var equipment = await _equipmentProvider.GetEquipmentByWkeid(WellKnownEntityId.Parse(equipmentWkeId));

                    if (equipment == null)
                    {
                        throw new NotFoundException("Equipment not found: " + equipmentWkeId) { ErrorCode = ErrorCodes.EquipmentNotFound };
                    }
                }

                await _episodeService.UpdateEpisode(episode);
                return NoContent();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Delete episode
        /// </summary>
        /// <param name="id">Episode id.</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider), 404)]
        public async Task<IActionResult> DeleteEpisode(string id)
        {
            try
            {
                var episode = await _episodeService.GetEpisodeById(id);

                foreach (var equipmentWkeId in episode.EquipmentWkeIdList)
                {
                    if (await _episodicPointService.AnyPointsExists(episode.Id, equipmentWkeId))
                    {
                        throw new BadRequestException("Can't delete episode with episodic points for equipmentId " + equipmentWkeId);
                    }
                }

                await _episodeService.DeleteEpisode(id);
                return Ok();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get episodic points for every equipment in given episode
        /// </summary>
        /// <param name="id">Episode id.</param>
        /// <param name="start">Time period start (accepts multiple formats).</param>
        /// <param name="end">Time period end (accepts multiple formats).</param>
        /// <returns></returns>
        [HttpGet("{id}/episodic-points")]
        [ProducesOkResponseType(typeof(EpisodeRows))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForDateExampleProvider), 400)]
        public async Task<IActionResult> GetEpisodicPoints(string id, string start, string end)
        {
            try
            {
                return Ok(await _apiImplementation.GetEpisodeRows(id, ParseTimePeriod(start, end), DataType.Episodic));
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get raw channel data for every equipment in given episode within its period
        /// </summary>
        /// <param name="id">Episode id.</param>
        /// <returns></returns>
        [HttpGet("{id}/channels")]
        [ProducesOkResponseType(typeof(EpisodeRows))]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        [ResponseBodyExampleProviderReference(typeof(EpisodeIdNotFoundExampleProvider), 404)]
        public async Task<IActionResult> GetChannels(string id)
        {
            try
            {
                return Ok(await _apiImplementation.GetEpisodeRows(id, null, DataType.Channel));
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        
        private bool IsAvatarChildEpisode(Episode episode)
        {
            if (episode.Type == null)
            {
                return false;
            }


            if (episode.Type.Equals("coring", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void ValidateAvatarChildEpisode(Episode episode)
        {
            if (episode.ParentId == null)
            {
                throw new BadRequestException("Avatar child episode should have ParentId of parent episode.");
            }

            if (episode.EquipmentWkeIdList.Count != 1)
            {
                throw new BadRequestException("Avatar child episode should have exactly one wkeid.");
            }

            if (episode.Tags.FindIndex(tag => tag.StartsWith("avatar-id:")) < 0)
            {
                throw new BadRequestException("Avatar child episode should have 'avatar-id:value' tag");
            }

            if (episode.Tags.FindIndex(tag => tag.StartsWith("avatar-run-id:")) < 0)
            {
                throw new BadRequestException("Avatar child episode should have 'avatar-run-id:value' tag");
            }

            if (episode.Tags.FindIndex(tag => tag.StartsWith("slb-correlation-id:")) < 0)
            {
                throw new BadRequestException("Avatar child episode should have 'slb-correlation-id:value' tag");
            }
        }

        private async Task NotifyODM(Episode episodeParsed)
        {
            try
            {
                Logger.Information("Notifying ODM.");

                string tag = episodeParsed.Tags.Single(x => x.StartsWith("slb-correlation-id:"));
                string slbCorrelationid = tag.Replace("slb-correlation-id:", null);

                JObject response = await SendRequestToODM(episodeParsed.EquipmentWkeIdList.Single(), slbCorrelationid);
                string json = Environment.NewLine + response.ToString(Formatting.Indented) + Environment.NewLine;

                Logger.Information(json);
            }
            catch (ServerErrorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ServerErrorException(ex.Message, ex);
            }
        }

        private async Task<JObject> SendRequestToODM(string wkeid, string slbCorrelationId)
        {
            var httpClient = new HttpClient(new HttpClientHandler());
            httpClient.DefaultRequestHeaders.Add("x-apikey", _config.OdmApi.XApiKey);

            // string json = "{\"wkeid\": \"" + wkeid + "\"}";

            string json = "{\"wkeid\": \"" + wkeid + "\", \"slbCorrelationId\": \"" + slbCorrelationId + "\"}";

            Logger.Information(Environment.NewLine);
            Logger.Information(json);
            Logger.Information(Environment.NewLine);

            var content = new StringContent(json);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            var response = await httpClient.PostAsync(_config.OdmApi.BaseUrl, content);
            string text = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var jobject = JObject.Parse(text);
                return jobject;
            }

            throw new ServerErrorException("Error calling ODM API: " + response.ReasonPhrase + " " + text);
        }


    }
}
