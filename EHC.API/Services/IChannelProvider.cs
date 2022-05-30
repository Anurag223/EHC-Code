using System;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Services;

namespace TLM.EHC.API.Services
{
    // not used, at least for now
    // the idea was using two sources for channel definitions: equipment model and global catalog

    public interface IChannelProvider
    {
        Task<ChannelDefinition> GetChannelByCode(string channelCode, string equipmentCode);
    }

    public class ChannelProvider : IChannelProvider
    {
        private readonly IChannelDefinitionService _channelDefinitionService;
        private readonly IEquipmentModelProvider _equipmentModelProvider;

        public ChannelProvider(
            IChannelDefinitionService channelDefinitionService,
            IEquipmentModelProvider equipmentModelProvider
        ){
            _channelDefinitionService = channelDefinitionService;
            _equipmentModelProvider = equipmentModelProvider;
        }

        public async Task<ChannelDefinition> GetChannelByCode(string channelCode, string equipmentCode)
        {
            // try find channel in equipment model
            var equipmentModel = await _equipmentModelProvider.GetEquipmentModelByCode(equipmentCode);

            var channelFromModel = equipmentModel?.Channels.SingleOrDefault(
                x => x.Code.Equals(channelCode, StringComparison.InvariantCultureIgnoreCase));

            if (channelFromModel != null)
            {
                return new ChannelDefinition()
                {
                    Code = channelFromModel.Code,
                    Name = channelFromModel.Name,
                    Dimension = channelFromModel.Dimension,
                    Uom = channelFromModel.Uom,
                    LegalClassification = channelFromModel.LegalClassification
                };
            }

            // try find channel in global catalog
            var channelFromCatalog = await _channelDefinitionService.GetChannelDefinition(channelCode);

            if (channelFromCatalog != null)
            {
                return channelFromCatalog;
            }

            return new ChannelDefinition()
            {
                Code = channelCode,
                Uom = "-",
                Dimension = "-"
            };
        }

    }
}
