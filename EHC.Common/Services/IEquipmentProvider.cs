using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TLM.EHC.Common.Clients.EquipmentApi;
using TLM.EHC.Common.Models;
using Equipment = TLM.EHC.Common.Models.Equipment;

namespace TLM.EHC.Common.Services
{
    public interface IEquipmentProvider
    {
        Task<Equipment> GetEquipmentByWkeid(WellKnownEntityId wkeid);
    }



    public class MateoEquipmentProvider : IEquipmentProvider
    {
        private readonly IEquipmentApiClient _equipmentApiClient;
        private readonly IMemoryCache _memoryCache;
        private readonly EhcApiConfig _apiConfig;

        public MateoEquipmentProvider(
            IEquipmentApiClient equipmentApiClient,
            IMemoryCache memoryCache,EhcApiConfig apiConfig)
        {
            _equipmentApiClient = equipmentApiClient;
            _memoryCache = memoryCache;
            _apiConfig = apiConfig;
        }


        public async Task<Equipment> GetEquipmentByWkeid(WellKnownEntityId wkeid)
        {
            if (_memoryCache.TryGetValue(wkeid.Value, out Equipment found))
            {
                return found;
            }

            var result = new Equipment();
            var equipment = await _equipmentApiClient.GetEquipmentByWkeId(wkeid.Value);

            if (equipment == null)
            {
                return null;
            }

            result.MaterialNumber = equipment.MaterialNumber;
            result.SerialNumber = equipment.SerialNumber;
            result.EquipmentCode = equipment.EquipmentCode;
            result.EquipmentWkeId = equipment.WellKnownEntityId;
            result.SourceSystemRecordId = equipment.SourceSystemRecordId;
            result.EpicClassifications = new List<Classification>();
            //Q: Is it possible to have classification not provided when storing equipment? i.e., should this be a nullable field
            //and with a check for null before we populate it?
            foreach(var classification in equipment.Classifications.Values.ToList())
            {
                result.EpicClassifications.AddRange(classification.ToList());
            }
            _memoryCache.Set(wkeid.Value, result, TimeSpan.FromHours(_apiConfig.EquipmentApi.CacheTimeDuration));
            return result;
        }
    }
}
