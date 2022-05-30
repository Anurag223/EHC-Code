using System;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.ADMIN.API.Services
{
    /// <summary>
    /// Interface for a2r utils audit Log service
    /// </summary>
    public interface IA2RUtilsAuditLogService
    {
        /// <summary>
        /// Get all a2r util logs from database collection
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        Task<CollectionResult<A2RUtilsAuditLog>> GetAllA2RUtilsAuditLog(QuerySpec spec);
        /// <summary>
        /// Insert a2r util log into database collection
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task<A2RUtilsAuditLog> CreateA2RUtilsAuditLog(A2RUtilsAuditLog log);
    }

    /// <summary>
    /// Service for a2r utils audit Log
    /// </summary>
    public class A2RUtilsAuditLogService : IA2RUtilsAuditLogService
    {
        private readonly IRepositoryHandler<A2RUtilsAuditLog> _repositoryHandler;

        /// <summary>
        /// Constructor- A2RUtilsAuditLogService
        /// </summary>
        /// <param name="repositoryHandler"></param>
        public A2RUtilsAuditLogService(
            IRepositoryHandler<A2RUtilsAuditLog> repositoryHandler
        )
        {
            _repositoryHandler = repositoryHandler;
        }

        /// <summary>
        /// Get all a2r util logs from database collection
        /// </summary>
        /// <param name="spec"></param>
        /// <returns></returns>
        public async Task<CollectionResult<A2RUtilsAuditLog>> GetAllA2RUtilsAuditLog(QuerySpec spec)
        {
            var a2RUtilsAuditLogs = await _repositoryHandler.QueryManyAsync(spec);
            if (a2RUtilsAuditLogs.Collection.Count == 0)
            {
                return null;
            }
            return a2RUtilsAuditLogs;
        }

        /// <summary>
        /// Insert a2r util log into database collection
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        /// <exception cref="BadRequestException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<A2RUtilsAuditLog> CreateA2RUtilsAuditLog(A2RUtilsAuditLog log)
        {
            if (log.Id != null)
            {
                throw new BadRequestException(EhcConstants.IdShouldBeNull);
            }
            await _repositoryHandler.UpdateAsync(log);
            return log;
        }

    }
}
