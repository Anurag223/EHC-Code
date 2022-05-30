using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.ADMIN.API.Services;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace TLM.EHC.ADMIN.API.Controllers
{
    public interface IAdminApiImplementation
    {
        Task<CollectionResult<InfluxDBMapping>> GetAllInfluxDBMappingData(QuerySpec spec);
        Task<bool> SetInfluxDbMappingStatus(string equipmentCode, bool status);
        Task<InfluxMappingResponse> CreateUpdateDbMap(string equipmentCode);

        Task<Dictionary<string, DBMapConflictStatus>> GetConflictStatusByEquipmentCode(CollectionResult<InfluxDBMapping> maps);
        Task<CollectionResult<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLog(QuerySpec spec);
        Task<CollectionResult<A2RUtilsAuditLog>> GetA2RUtilsAuditLog(QuerySpec spec);

        Task CreateDbInInflux(string equipmentCode);
        Task<A2RUtilsAuditLog> CreateA2RUtilsAuditLog(A2RUtilsAuditLog log);
    }
    public class AdminApiImplementation : IAdminApiImplementation
    {
        private readonly IInfluxDBMappingService influxDbMappingService;
        private readonly IEpicV3HierarchyProvider epicV3HierarchyProvider;
        private readonly IDBMapConflictLogService dBMapConflictLogService;
        private readonly IA2RUtilsAuditLogService a2rUtilsAuditLogService;
        private readonly IHistorianClient historianClient;


        public AdminApiImplementation(
           IEpicV3HierarchyProvider _epicV3HierarchyProvider,
            IInfluxDBMappingService _influxDbMappingService,
            IDBMapConflictLogService _dBMapConflictLogService,
             IA2RUtilsAuditLogService _a2rUtilsAuditLogService,
        IHistorianClient _historianClient
           )

        {
            influxDbMappingService = _influxDbMappingService;
            epicV3HierarchyProvider = _epicV3HierarchyProvider;
            dBMapConflictLogService = _dBMapConflictLogService;
            a2rUtilsAuditLogService = _a2rUtilsAuditLogService;
            historianClient = _historianClient;
        }

        public async Task<CollectionResult<InfluxDBMapping>> GetAllInfluxDBMappingData(QuerySpec spec)
        {
            return await influxDbMappingService.GetAllInfluxDBMappingData(spec);

        }
        public async Task<Dictionary<string, DBMapConflictStatus>> GetConflictStatusByEquipmentCode(CollectionResult<InfluxDBMapping> maps)
        {
            Dictionary<string, DBMapConflictStatus> eqCodeWithConflictStatus = new Dictionary<string, DBMapConflictStatus>();
            List<string> equipmentCodes = maps.Collection.SelectMany(o => o.EquipmentCodes).ToList();
            foreach (string code in equipmentCodes)
            {
                var conflictStatus = await dBMapConflictLogService.GetConflictStatusByEquipmentCode(code);
                eqCodeWithConflictStatus.Add(code, conflictStatus);
            }
            return eqCodeWithConflictStatus;
        }

        public async Task<bool> SetInfluxDbMappingStatus(string equipmentCode, bool status)
        {
            var result = await influxDbMappingService.SetInfluxDbMappingStatus(equipmentCode, status);
            return result;
        }

        public async Task CreateDbInInflux(string equipmentCode)
        {
            var dbMap = await influxDbMappingService.GetInfluxDBName(equipmentCode);
            await historianClient
                .CreateDatabase(dbMap.DbName); //Influx user (set in pipeline) should have Create permissions.
        }

        public async Task<InfluxMappingResponse> CreateUpdateDbMap(string equipmentCode)
        {
            var epicV3Wkid = Convert.ToString((int)EpicV3ClassificationType.EquipmentToolset) + ":" +
                            equipmentCode;

            EquipmentModel equipmentModel = await epicV3HierarchyProvider.GetEpicHierarchyInfoFromEquipmentCode(epicV3Wkid, equipmentCode);

            var influxPath = InfluxPath.GetFromEquipmentModel(equipmentModel);

            if (influxPath == null)
            {
                throw new NotFoundException(EhcConstants.InfluxPathNotFound);
            }

            return await CreateDBMappingOrAddEquipmentCode(influxPath, equipmentModel);
        }

        private async Task<InfluxMappingResponse> CreateDBMappingOrAddEquipmentCode(InfluxPath influxPath, EquipmentModel equipmentModel)
        {
            //Insert the new mapping or add equipment code in Mongo DB lookup table also.
            InfluxDBMapping dbToCreate = new InfluxDBMapping();
            dbToCreate.BrandName = equipmentModel.BrandName;
            dbToCreate.TechnologyName = equipmentModel.TechnologyName;
            dbToCreate.DbName = influxPath.Technology;
            dbToCreate.MeasurementName = influxPath.Brand;
            dbToCreate.BrandCode = equipmentModel.BrandCode;
            dbToCreate.TechnologyCode = equipmentModel.TechnologyCode;
            dbToCreate.EquipmentCodes = new List<string> { influxPath.EquipmentCode };
            dbToCreate.Status = InfluxDBStatus.Disabled;

            return await influxDbMappingService.CreateUpdateDBMapping(dbToCreate);
        }

        public async Task<CollectionResult<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLog(QuerySpec spec)
        {
            var epicDBMapConflicts = await dBMapConflictLogService.GetAllEpicDBMapConflictLogByCriteria(spec);
            if (epicDBMapConflicts != null && epicDBMapConflicts.Collection.Any())
            {
                var distinctConflicts = epicDBMapConflicts.Collection.OrderByDescending(e => e.CreatedDate)
                    .DistinctBy(o => o.DBMapEquipmentCode).ToList();

                if (distinctConflicts != null && distinctConflicts.Any())
                {
                    var conflictsList = await dBMapConflictLogService.GetAllEpicDBMapConflictLogs();
                    if (conflictsList != null && conflictsList.Any())
                    {
                        var eqCodeDataWithConflictStartDate = GetConflictStartDateWithDBMapEquipmentCode(conflictsList);

                        foreach (var data in distinctConflicts)
                        {
                            if (eqCodeDataWithConflictStartDate.ContainsKey(data.DBMapEquipmentCode))
                            {
                                data.ConflictStartDate = eqCodeDataWithConflictStartDate[data.DBMapEquipmentCode];
                            }
                        }
                    }
                    epicDBMapConflicts = new CollectionResult<EpicDBMapConflictLog>(distinctConflicts, null);
                }
            }
            return epicDBMapConflicts;
        }

        public async Task<CollectionResult<A2RUtilsAuditLog>> GetA2RUtilsAuditLog(QuerySpec spec)
        {
            return await a2rUtilsAuditLogService.GetAllA2RUtilsAuditLog(spec);
        }

        public async Task<A2RUtilsAuditLog> CreateA2RUtilsAuditLog(A2RUtilsAuditLog log)
        {
            return await a2rUtilsAuditLogService.CreateA2RUtilsAuditLog(log);
        }
        private static Dictionary<string, string> GetConflictStartDateWithDBMapEquipmentCode(List<EpicDBMapConflictLog> conflicts)
        {
            Dictionary<string, string> eqCodeDataWithConflictStartDate = new Dictionary<string, string>();
            var eqCodesWithConflictDate = conflicts.OrderBy(e => e.CreatedDate)
                .DistinctBy(o => o.DBMapEquipmentCode)
                .Select(x => new { x.DBMapEquipmentCode, x.CreatedDate });
            foreach (var item in eqCodesWithConflictDate)
            {
                if (!eqCodeDataWithConflictStartDate.ContainsKey(item.DBMapEquipmentCode))
                    eqCodeDataWithConflictStartDate.Add(item.DBMapEquipmentCode, item.CreatedDate.ToString());
            }
            return eqCodeDataWithConflictStartDate;
        }
    }
}
