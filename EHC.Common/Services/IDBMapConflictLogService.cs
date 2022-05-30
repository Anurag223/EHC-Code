using Microsoft.Extensions.Caching.Memory;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.Common.Services
{
    public interface IDBMapConflictLogService
    {
        Task<List<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLogs();
        Task<DBMapConflictStatus> GetConflictStatusByEquipmentCode(string equipmentCode, bool noCache = false);
        Task<CollectionResult<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLogByCriteria(QuerySpec spec);

    }

    public class DBMapConflictLogService : IDBMapConflictLogService
    {
        private readonly IRepositoryHandler<EpicDBMapConflictLog> _repositoryHandler;
        private readonly IMemoryCache _memoryCache;

        public DBMapConflictLogService(
            IRepositoryHandler<EpicDBMapConflictLog> repositoryHandler,
              IMemoryCache memoryCache
        )
        {
            _repositoryHandler = repositoryHandler;
            _memoryCache = memoryCache;

        }

        public async Task<List<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLogs()
        {
            var epicDBMapConflicts = await _repositoryHandler.GetAsync(x => x.ConflictStatus == DBMapConflictStatus.OutOfSync);
            if (epicDBMapConflicts.Count == 0)
            {
                return null;
            }
            return epicDBMapConflicts;
        }
        public async Task<CollectionResult<EpicDBMapConflictLog>> GetAllEpicDBMapConflictLogByCriteria(QuerySpec spec)
        {
           var epicDBMapConflicts = await _repositoryHandler.QueryManyAsync(spec);
            if (epicDBMapConflicts.Collection.Count == 0)
            {
                return null;
            }
            return epicDBMapConflicts;
        }

        public async Task<DBMapConflictStatus> GetConflictStatusByEquipmentCode(string equipmentCode, bool noCache = false)
        {
            if (string.IsNullOrEmpty(equipmentCode))
            {
                throw new BadRequestException(EhcConstants.EquipmentCodeCannotBeNullOrEmpty);
            }
            if (!noCache && _memoryCache.TryGetValue(equipmentCode, out DBMapConflictStatus found))
            {
                return found;
            }

            var list = await _repositoryHandler.GetAsync(x => x.DBMapEquipmentCode == equipmentCode);

            if (list.Count == 0)
            {
                _memoryCache.Set(equipmentCode, DBMapConflictStatus.InSync, TimeSpan.FromDays(1));
                return DBMapConflictStatus.InSync;
            }
            else
            {
                _memoryCache.Set(equipmentCode, DBMapConflictStatus.OutOfSync, TimeSpan.FromDays(1));
                return DBMapConflictStatus.OutOfSync;
            }

        }

      

    }
}
