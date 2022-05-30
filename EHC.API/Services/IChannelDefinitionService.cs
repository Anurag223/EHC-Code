using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;



namespace TLM.EHC.API.Services
{
    // global channel catalog
    // temporarily part of this solution

    public interface IChannelDefinitionService
    {
        Task<ChannelDefinition> CreateChannelDefinition(ChannelDefinition channelDefinition);
        Task UpdateChannelDefinition(ChannelDefinition channelDefinition);
        Task DeleteChannelDefinition(string code);
        Task<ChannelDefinition> GetChannelDefinition(string code, bool noCache = false);

        Task<ChannelDefinitionClean> GetChannelDescription(string field);
        Task<string> GetFieldName(string code);
        string GetFieldName(ChannelDefinition channelDefinition);
        Task UpdateEquipmentCodeOnChannelDefinition(string eqCode, List<string> channelEqCodes);
        Task ValidateChannelCode(List<string> channelCodes);
        Task ValidateEquipmentCode(string equipmentCode);
    }

    public class ChannelDefinitionService : IChannelDefinitionService
    {
        private readonly IRepositoryHandler<ChannelDefinition> _repositoryHandler;
        private readonly IMemoryCache _memoryCache;
        private readonly IEpicV3HierarchyProvider _epicV3HierarchyProvider;
        private readonly EhcApiConfig _apiConfig;

        public ChannelDefinitionService(
            IRepositoryHandler<ChannelDefinition> repositoryHandler,
            IMemoryCache memoryCache,
            EhcApiConfig apiConfig, IEpicV3HierarchyProvider epicV3HierarchyProvider
        )
        {
            _repositoryHandler = repositoryHandler;
            _memoryCache = memoryCache;
            _epicV3HierarchyProvider = epicV3HierarchyProvider;
            _apiConfig = apiConfig;
        }


        public async Task<ChannelDefinition> CreateChannelDefinition(ChannelDefinition channelDefinition)
        {

            if (channelDefinition.Id != null)
            {
                throw new BadRequestException("ChannelDefinition.Id should be null.");
            }

            await Validate(channelDefinition);

            var found = await GetChannelDefinition(channelDefinition.Code, true);

            if (found != null)
            {
                throw new BadRequestException("ChannelDefinition already exists: " + channelDefinition.Code);
            }

            await _repositoryHandler.UpdateAsync(channelDefinition);

            if (string.IsNullOrWhiteSpace(channelDefinition.Id))
            {
                throw new Exception("Got empty id after creating an episode.'");
            }

            return channelDefinition;
        }

        public async Task UpdateChannelDefinition(ChannelDefinition channelDefinition)
        {
            await Validate(channelDefinition);

            var found = await GetChannelDefinition(channelDefinition.Code, true);

            if (found == null)
            {
                throw new NotFoundException("ChannelDefinition not found: " + channelDefinition.Code) { ErrorCode = ErrorCodes.ChannelDefinitionNotFound };
            }

            channelDefinition.Id = found.Id;
            await _repositoryHandler.UpdateAsync(channelDefinition);
            if (_memoryCache.TryGetValue(channelDefinition.Code, out _))
            {
                _memoryCache.Remove(channelDefinition.Code);
            }
        }

        public async Task DeleteChannelDefinition(string code)
        {
            var found = await GetChannelDefinition(code, true);

            if (found == null)
            {
                throw new NotFoundException("ChannelDefinition not found: " + code)
                { ErrorCode = ErrorCodes.ChannelDefinitionNotFound };
            }

            await _repositoryHandler.DeleteManyAsync(DeleteSpec.ById(found.Id));
            _memoryCache.Remove(code);
        }

        public async Task<ChannelDefinition> GetChannelDefinition(string code, bool noCache)
        {
            if (!noCache && _memoryCache.TryGetValue(code, out ChannelDefinition found))
            {
                return found;
            }

            var list = await _repositoryHandler.GetAsync(x => x.Code == code);

            if (list.Count == 0)
            {
                return null;
            }

            var channelDefinition = list.Single();

            _memoryCache.Set(code, channelDefinition, TimeSpan.FromHours(_apiConfig.ServiceCacheTimeDuration));
            return channelDefinition;
        }

        public async Task UpdateEquipmentCodeOnChannelDefinition(string eqCode, List<string> inputChannelCodes)
        {
            var dbChannelDef = await GetChannelDefinitionsByEquipmentCode(eqCode);
            var commonChannelCodes = dbChannelDef.Select(s1 => s1.Code).ToList().Intersect(inputChannelCodes).ToList();
            var toRemoveDbChannelDef = dbChannelDef.Select(s1 => s1.Code).ToList().Except(commonChannelCodes).ToList();
            var toBeUpdatedChannelCodes = inputChannelCodes.Except(commonChannelCodes).ToList();
            var channelDefFromWhichEqCodeToBeAdded = await GetChannelDefinitionByChannelCode(toBeUpdatedChannelCodes);

            if (toRemoveDbChannelDef.Any())
            {
                List<ChannelDefinition> channelDefFromWhichEqCodeToBeDeleted =
                    dbChannelDef.Where(cd => toRemoveDbChannelDef.Contains(cd.Code)).ToList();
                await RemoveEqCodeFromChannelDefinition(channelDefFromWhichEqCodeToBeDeleted, eqCode);
            }

            if (channelDefFromWhichEqCodeToBeAdded.Any())
                await AddEqCodeOnChannelDefinition(channelDefFromWhichEqCodeToBeAdded, eqCode);
        }

        // for reading data
        public async Task<string> GetFieldName(string code)
        {
            var channelDefinition = await this.GetChannelDefinition(code, false);

            if (channelDefinition == null)
            {
                return code;
            }

            return GetFieldName(channelDefinition);
        }

        // for writing data
        public string GetFieldName(ChannelDefinition channelDefinition)
        {
            if (string.IsNullOrWhiteSpace(channelDefinition.Uom))
            {
                throw new ArgumentException("Empty UOM in channel definition for code: " + channelDefinition.Code);
            }

            if (string.Equals(channelDefinition.Uom, "unitless", StringComparison.InvariantCultureIgnoreCase))
            {
                return channelDefinition.Code;
            }

            return channelDefinition.Code + "." + channelDefinition.Uom;
        }

        public async Task<ChannelDefinitionClean> GetChannelDescription(string field)
        {
            string code = field;
            string uom = null;

            int index = field.LastIndexOf('.'); // AirPressure.kPa

            if (index > 0)
            {
                code = field.Substring(0, index); // AirPressure
                uom = field.Substring(index + 1); // kPa
            }

            var channelDefinition = await this.GetChannelDefinition(code, false);

            if (channelDefinition == null)
            {
                return new ChannelDefinitionClean() { Code = code, Uom = uom };
            }

            if (channelDefinition.Uom != uom)
            {
                // raise exception on mismatch?
            }

            return new ChannelDefinitionClean(channelDefinition);
        }

        public async Task ValidateEquipmentCode(string equipmentCode)
        {
            await ValidateEquipmentCodeFromEpicV3(equipmentCode);
        }

        public async Task ValidateChannelCode(List<string> channelCodes)
        {
            foreach (var channelCode in channelCodes)
            {
                var channelData = await GetChannelDefinition(channelCode, true);
                if (channelData == null)
                {
                    throw new NotFoundException(
                        EhcConstants.ChannelDefinitionNotFoundForCode + channelCode)
                    { ErrorCode = ErrorCodes.ChannelCodeNotFound };
                }
            }
        }

        private async Task<List<ChannelDefinition>> GetChannelDefinitionsByEquipmentCode(string equipmentcode)
        {

            List<string> eqCode = new List<string> { equipmentcode };
            var list = await _repositoryHandler.QueryManyAsync(QuerySpec.ByValues("equipmentcodes", eqCode));
            return list.Collection.ToList();
        }

        private async Task<List<ChannelDefinition>> GetChannelDefinitionByChannelCode(List<string> channelCodes)
        {
            var list = await _repositoryHandler.QueryManyAsync(QuerySpec.ByValues("code", channelCodes));
            return list.Collection.ToList();

        }

        private async Task ValidateEquipmentCodeFromEpicV3(string equipmentCode)
        {
            var epicV3WkId = Convert.ToString((int)EpicV3ClassificationType.EquipmentToolset) + ":" +
                             equipmentCode;
            await _epicV3HierarchyProvider.GetEpicHierarchyInfoFromCode(epicV3WkId);
        }

        private async Task RemoveEqCodeFromChannelDefinition(List<ChannelDefinition> channelDefinitions, string eqCode)
        {
            channelDefinitions.ForEach(u =>
            {
                if (_memoryCache.TryGetValue(u.Code, out _))
                {
                    _memoryCache.Remove(u.Code);
                }
                u.EquipmentCodes.Remove(eqCode);

            });
            await _repositoryHandler.UpdateManyAsync(channelDefinitions);

        }

        private async Task AddEqCodeOnChannelDefinition(List<ChannelDefinition> channelDefinitions, string eqCode)
        {
            channelDefinitions.ForEach(u =>
            {
                if (_memoryCache.TryGetValue(u.Code, out _))
                {
                    _memoryCache.Remove(u.Code);
                }

                if (u.EquipmentCodes != null)
                {
                    u.EquipmentCodes.Add(eqCode);
                }
                else
                {
                    u.EquipmentCodes = new List<string>() { eqCode };
                }

            });

            await _repositoryHandler.UpdateManyAsync(channelDefinitions);

        }

        private async Task Validate(ChannelDefinition channelDefinition)
        {
            if (string.IsNullOrWhiteSpace(channelDefinition.Code))
            {
                throw new BadRequestException("Empty ChannelDefinition.Code");
            }

            if (string.IsNullOrWhiteSpace(channelDefinition.Dimension))
            {
                throw new BadRequestException("Empty ChannelDefinition.Dimension");
            }

            if (string.IsNullOrWhiteSpace(channelDefinition.Uom))
            {
                throw new BadRequestException("Empty ChannelDefinition.Uom");
            }
            if (channelDefinition.EquipmentCodes != null)
            {
                foreach (string eqCode in channelDefinition.EquipmentCodes)
                {
                    await ValidateEquipmentCode(eqCode);
                }
            }


        }
    }
}

