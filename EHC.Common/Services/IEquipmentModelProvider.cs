using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TLM.EHC.Common.Clients.EquipmentModelApi;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using EquipmentModel = TLM.EHC.Common.Models.EquipmentModel;

namespace TLM.EHC.Common.Services
{
    public interface IEquipmentModelProvider
    {
        Task<EquipmentModel> GetEquipmentModelByCode(string equipmentCode);
    }


    public class EquipmentModelProvider : IEquipmentModelProvider
    {
        private readonly IEquipmentModelApiClient _equipmentModelApiClient;
        private readonly IMemoryCache _memoryCache;

        public EquipmentModelProvider(
            IEquipmentModelApiClient equipmentModelApiClient,
            IMemoryCache memoryCache
        ){
            _equipmentModelApiClient = equipmentModelApiClient;
            _memoryCache = memoryCache;
        }

        public async Task<EquipmentModel> GetEquipmentModelByCode(string equipmentCode)
        {
            if (_memoryCache.TryGetValue(equipmentCode, out EquipmentModel found))
            {
                return found;
            }

            Clients.EquipmentModelApi.EquipmentModel model;
            ICollection<ChannelReference> channels;

            try
            {
                model = await _equipmentModelApiClient.GetByEquipmentCodeAsync(equipmentCode, null);

                // we may use channel definitions from equipment model in future
                // it will override global channel definition for the same code
                // see IChannelProvider
                // channels = await _equipmentModelApiClient.GetEquipmentModelChannelsByEquipmentCodeAsync(equipmentCode);
            }
            catch (Exception ex)
            {
                if (ex is ApiException apiException && apiException.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    throw new ServerErrorException("Equipment Model API - Code not found: " + equipmentCode);
                }

                throw new ServerErrorException("Error requesting external Equipment Model API.", ex);
            }

            var result = new EquipmentModel();
            result.EquipmentCode = model.EquipmentCode;
            result.Description = model.Description;
            result.MaterialNumber = model.MaterialNumber;
            result.TechnologyCode = model.TechnologyCode;
            result.BrandCode = model.BrandCode;
            result.TechnologyName = model.TechnologyName;
            result.BrandName = model.BrandName;

            // result.Channels = channels.Select(ConvertChannel).ToList();

            // TODO put time period into config
            _memoryCache.Set(equipmentCode, result, TimeSpan.FromMinutes(5));
            return result;
        }


        private EquipmentModelChannel ConvertChannel(ChannelReference channel)
        {
            return new EquipmentModelChannel()
            {
                Code = channel.Code,
                Name = channel.Name,
                Dimension = channel.Dimension,
                Uom = channel.Uom,
                LegalClassification = channel.LegalClassification,
            };
        }
    }
}
