using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Data;
using TLM.EHC.Admin;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Core.Models.Querying;

namespace TLM.EHC.Common.Services
{
    public interface IInfluxDBMappingService
    {
        Task<InfluxMappingResponse> CreateUpdateDBMapping(InfluxDBMapping influxDBMapping);
        Task<InfluxDBMapping> GetInfluxDBMapping(string code, bool noCache = false);

        Task<InfluxDBMapping> GetInfluxDBName(string equipmentCode);

        Task<CollectionResult<InfluxDBMapping>> GetAllInfluxDBMappingData(QuerySpec spec);

        Task<bool> SetInfluxDbMappingStatus(string equipmentCode, bool status);

    }

    public class InfluxDBMappingService : IInfluxDBMappingService
    {
        private readonly IRepositoryHandler<InfluxDBMapping> _repositoryHandler;
        private readonly IMemoryCache _memoryCache;
        private readonly EhcApiConfig _apiConfig;
       
        public InfluxDBMappingService(
            IRepositoryHandler<InfluxDBMapping> repositoryHandler,
            IMemoryCache memoryCache,EhcApiConfig apiConfig
        )
        {
            _repositoryHandler = repositoryHandler;
            _memoryCache = memoryCache;
            _apiConfig = apiConfig;
        }

        public async Task<InfluxMappingResponse> CreateUpdateDBMapping(InfluxDBMapping influxDBMapping)
        {
            InfluxMappingResponse response = new InfluxMappingResponse();
            Validate(influxDBMapping);
            FormatNames(influxDBMapping);

            var foundEqCode = await GetInfluxDBMapping(influxDBMapping.EquipmentCodes[0], true);

            if (foundEqCode != null)
            {
                response.IsNewMeasurement = false;
                response.DBStatus = foundEqCode.Status;
                response.MessageForAdmin = EhcConstants.EquipmentCodeAlreadyExists;
                return response;
            }

            var foundBrandName = await GetInfluxBrandName(influxDBMapping.BrandName);

            if (foundBrandName != null)
            {
                //update Equipment code in same record.
                foundBrandName.EquipmentCodes.Add(influxDBMapping.EquipmentCodes[0]);
                await _repositoryHandler.UpdateAsync(foundBrandName);              
                response.IsNewMeasurement = false;
                response.MeasurementName = foundBrandName.MeasurementName;
                response.DBName = foundBrandName.DbName;
                response.DBStatus = foundBrandName.Status;
                response.MessageForAdmin = "Equipmentcode added in existing influxdb mapping";
                return response;
            }

            if (!await _repositoryHandler.UpdateAsync(influxDBMapping))
                return null;
            response.IsNewMeasurement = true;
            response.DBStatus = influxDBMapping.Status;
            response.MessageForAdmin= "New influxdb mapping created successfully for equipment code:" + influxDBMapping.EquipmentCodes[0]; 
            if (string.IsNullOrWhiteSpace(influxDBMapping.Id))
            {
                throw new Exception("Got empty id after creating a mapping in DB.'");
            }
            return response;
        }

        public async Task<InfluxDBMapping> GetInfluxDBMapping(string code, bool noCache)
        {
            if (!noCache && _memoryCache.TryGetValue(code, out InfluxDBMapping found))
            {
                return found;
            }

            var list = await _repositoryHandler.GetAsync(x => x.EquipmentCodes.Contains(code));

            if (list.Count == 0)
            {
                return null;
            }

            var influxDBMapping = list.Single();

            _memoryCache.Set(code, influxDBMapping, TimeSpan.FromHours(_apiConfig.ServiceCacheTimeDuration));
            return influxDBMapping;
        }

        public async Task<InfluxDBMapping> GetInfluxDBName(string equipmentCode)
        {
            var list = await _repositoryHandler.GetAsync(x => x.EquipmentCodes.Contains(equipmentCode));

            if (list.Count == 0)
            {
                throw new NotFoundException(EhcConstants.EquipmentCodeNotInDBMap);
            }
            return list.Single();
        }

        public async Task<CollectionResult<InfluxDBMapping>> GetAllInfluxDBMappingData(QuerySpec spec)
        {

            var influxDBMapping = await _repositoryHandler.QueryManyAsync(spec);
            if (influxDBMapping.Collection.Count == 0)
            {
                return null;
            }
            return influxDBMapping;
        }

        public async Task<bool> SetInfluxDbMappingStatus(string equipmentCode, bool status)
        {
            var influxPath = await GetInfluxDBMapping(equipmentCode, true);

            if (influxPath == null)
            {
                throw new NotFoundException(EhcConstants.EquipmentCodeNotInDBMap);
            }

            influxPath.Status = status ? InfluxDBStatus.Enabled : InfluxDBStatus.Disabled;
            var result = await _repositoryHandler.UpdateAsync(influxPath);
            return result;
        }
      
        private void FormatNames(InfluxDBMapping influxDBMapping)
        {
            influxDBMapping.BrandName = influxDBMapping.BrandName.Replace('_', ' ');
            influxDBMapping.TechnologyName = influxDBMapping.TechnologyName.Replace('_', ' ');
        }

        private async Task<InfluxDBMapping> GetInfluxBrandName(string brandName)
        {
            var list = await _repositoryHandler.GetAsync(x => x.BrandName == brandName);

            if (list.Count == 0)
            {
                return null;
            }
            return list.Single();

        }

        private void Validate(InfluxDBMapping influxDBMapping)
        {
            if (string.IsNullOrWhiteSpace(influxDBMapping.DbName))
            {
                throw new BadRequestException("Empty DB name");
            }

            if (influxDBMapping.EquipmentCodes == null)
            {
                throw new BadRequestException("No equipment codes");
            }

            if (string.IsNullOrWhiteSpace(influxDBMapping.MeasurementName))
            {
                throw new BadRequestException("Empty Measurement Name");
            }
        }
    }
}
