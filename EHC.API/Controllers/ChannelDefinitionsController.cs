using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tlm.Sdk.Api;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Services;
using TLM.EHC.API.Swagger.ResponseBodyExampleProviders;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Core.Models.Querying;

namespace TLM.EHC.API.Controllers
{
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("v2/channel-definitions")]
    [RootPolicy]

    public class ChannelDefinitionsController : BaseController
    {
        private readonly IChannelDefinitionService _channelDefinitionService;
        private readonly IGetCollectionFromCacheStrategy<ChannelDefinition> _multiResourceGetter;

        public ChannelDefinitionsController(
            IChannelDefinitionService channelDefinitionService,
            IGetCollectionFromCacheStrategy<ChannelDefinition> multiResourceGetter
        ) : base(null)
        {
            _channelDefinitionService = channelDefinitionService;
            _multiResourceGetter = multiResourceGetter;
        }

        /// <summary>
        /// Get all global channel definitions (channel catalog)
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns>The global channel definition</returns>
        [HttpGet]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<ChannelDefinition>))]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForPageSizeAndNumberExampleProvider), 400)]
        public async Task<IActionResult> GetChannelDefinitions([QuerySpecBinder(typeof(ChannelDefinition), ExclusionPolicies = QueryExclusionPolicies.None, Key = nameof(ChannelDefinitionsController))] QuerySpec querySpec)
        {
            return await _multiResourceGetter.GetCollection(querySpec);
        }

        /// <summary>
        /// Add new global channel definition
        /// </summary>
        /// <param name="channelDefinition">Specify Channel Definition in json format.See Example</param>
        /// <returns></returns>

        [HttpPost]
        [ConsumesApplicationJson]
        [ProducesBadRequestResponseType]
        [ProducesNotFoundResponseType]
        [ProducesOkResponseType(typeof(ChannelDefinition))]
        [ResponseBodyExampleProviderReference(typeof(EquipmentCodeNotFoundExampleProvider), 404)]
        public async Task<IActionResult> CreateChannelDefinition([FromBody] ChannelDefinition channelDefinition)
        {
            try
            {
                if (channelDefinition == null) throw new BadRequestException("Invalid json");
                var result = await _channelDefinitionService.CreateChannelDefinition(channelDefinition);
                return Ok(result);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Update global channel definition
        /// </summary>
        /// <param name="code">Specify the channel code.</param>
        /// <param name="channelDefinition">Specify Channel definition in json format.See example</param>
        /// <returns></returns>
        [HttpPut("{code}")]
        [ConsumesApplicationJson]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesNoContentResponseType]
        [ResponseBodyExampleProviderReference(typeof(BadRequestForCodeExampleProvider), 400)]
        [ResponseBodyExampleProviderReference(typeof(ChannelDefinitionNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentCodeNotFoundExampleProvider), 404)]
        public async Task<IActionResult> UpdateChannelDefinition(string code, [FromBody] ChannelDefinition channelDefinition)
        {
            try
            {
                if (channelDefinition == null) throw new BadRequestException("Invalid json");
                if (channelDefinition.Code != code)
                {
                    throw new BadRequestException("Mismatching ChannelDefinition.Code");
                }

                await _channelDefinitionService.UpdateChannelDefinition(channelDefinition);
                return NoContent();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

        /// <summary>
        /// Get global channel definition by code
        /// </summary>
        /// <param name="code">Specify the channel code.</param>
        /// <returns></returns>

        [HttpGet("{code}")]
        [ProducesNotFoundResponseType]
        [ProducesOkResponseType(typeof(ChannelDefinition))]
        [ResponseBodyExampleProviderReference(typeof(ChannelDefinitionNotFoundExampleProvider), 404)]
        public async Task<IActionResult> GetChannelDefinitionByCode(string code)
        {
            try
            {
                var found = await _channelDefinitionService.GetChannelDefinition(code);

                if (found == null)
                {
                    throw new NotFoundException("ChannelDefinition not found: " + code) { ErrorCode = ErrorCodes.ChannelDefinitionNotFound };
                }

                return Ok(found);
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Delete global channel definition
        /// </summary>
        /// <param name="code">Specify the channel code.</param>
        /// <returns></returns>
        [HttpDelete("{code}")]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ResponseBodyExampleProviderReference(typeof(ChannelDefinitionNotFoundExampleProvider), 404)]
        public async Task<IActionResult> DeleteChannelDefinition(string code)
        {
            try
            {
                await _channelDefinitionService.DeleteChannelDefinition(code);
                return Ok();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }


        /// <summary>
        /// Bulk update channel definitions with equipment code for provided set of channel code
        /// </summary>
        /// <param name="equipmentCode">Equipment Code</param>        
        /// <param name="channelCodes">List of channel codes</param>
        /// <returns></returns>
        [HttpPost("bulkupdatechannels/{equipmentCode}")]
        [ConsumesApplicationJson]
        [ProducesNotFoundResponseType]
        [ProducesBadRequestResponseType]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ResponseBodyExampleProviderReference(typeof(ChannelDefinitionNotFoundExampleProvider), 404)]
        [ResponseBodyExampleProviderReference(typeof(EquipmentCodeNotFoundExampleProvider), 404)]
        public async Task<IActionResult> UpdateChannelDefinitionWithEqCodes(string equipmentCode,[FromBody] List<string> channelCodes)
        {
            try
            {
                if (channelCodes == null) throw new BadRequestException("Invalid json");
                await _channelDefinitionService.ValidateEquipmentCode(equipmentCode);
                await _channelDefinitionService.ValidateChannelCode(channelCodes);
                await _channelDefinitionService.UpdateEquipmentCodeOnChannelDefinition(equipmentCode, channelCodes);
                return Ok();
            }
            catch (HttpStatusException ex)
            {
                return CreateHttpStatus(ex);
            }
        }

    }
}
