using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TLM.EHC.Common.Clients.EpicV3Api;
using TLM.EHC.Common.Models;
using Equipment = TLM.EHC.Common.Models.Equipment;

namespace TLM.EHC.Common.Services
{
    public interface IEpicV3HierarchyProvider
    {
        Task<EpicV3Hierarchy> GetEpicHierarchyInfoFromCode(string code);
        Task<EquipmentModel> GetEpicHierarchyInfoFromEquipmentCode(string code, string equipmentCode);
    }



    public class EpicV3HierarchyProvider : IEpicV3HierarchyProvider
    {
        private readonly IEpicV3ApiClient _epicV3ApiClient;
        private readonly IMemoryCache _memoryCache;
        private readonly EhcApiConfig _apiConfig;

        public EpicV3HierarchyProvider(
            IEpicV3ApiClient epicV3ApiClient,
            IMemoryCache memoryCache,EhcApiConfig apiConfig)
        {
            _epicV3ApiClient = epicV3ApiClient;
            _memoryCache = memoryCache;
            _apiConfig = apiConfig;
        }


        public async Task<EpicV3Hierarchy> GetEpicHierarchyInfoFromCode(string code)
        {
            if (_memoryCache.TryGetValue(code, out EpicV3Hierarchy found))
            {
                return found;
            }

            var epicHierarchy = await _epicV3ApiClient.GetEpicHierarchyInfoFromCode(code);

            if (epicHierarchy == null)
            {
                return null;
            }
            EpicV3Hierarchy result = GetEpicV3Hierarchy(epicHierarchy);
            result.Children = new List<EpicV3Hierarchy>();
            foreach (var child in epicHierarchy.Children)
            {
               var childHierachy= GetEpicV3Hierarchy(child);
                result.Children.Add(childHierachy);
            }

            _memoryCache.Set(code, result, TimeSpan.FromHours(_apiConfig.EpicV3Api.CacheTimeDuration));
            return result;
        }

        public async Task<EquipmentModel> GetEpicHierarchyInfoFromEquipmentCode(string code, string equipmentCode)
        {

            var epicHierarchy = await _epicV3ApiClient.GetEpicHierarchyInfoForParent(code);
            if (epicHierarchy == null)
            {
                return null;
            }

            var techInfo = epicHierarchy.Item1;
            EpicV3Hierarchy result = GetEpicV3Hierarchy(techInfo);

            var brandInfo = epicHierarchy.Item2;

            EquipmentModel equipmentModel = new EquipmentModel();
            equipmentModel.TechnologyCode = techInfo.Code;
            equipmentModel.TechnologyName = FormatNameForHierarchy(techInfo.Name);
            equipmentModel.BrandCode = brandInfo.Code;
            equipmentModel.BrandName = FormatNameForHierarchy(brandInfo.Name);
            equipmentModel.EquipmentCode = equipmentCode;
            return equipmentModel;
        }


        private static EpicV3Hierarchy GetEpicV3Hierarchy(EpicRepresentationV3 epicHierarchy)
        {
            var result = new EpicV3Hierarchy();
            result.Code = epicHierarchy.Code;
            result.ParentCode = epicHierarchy.ParentCode;
            result.Name = FormatNameForHierarchy(epicHierarchy.Name);
            result.Type = (EpicV3ClassificationType)Enum.Parse(typeof(EpicV3ClassificationType), epicHierarchy.Type.ToString());
            return result;
        }

        private static string FormatNameForHierarchy(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                string split = "]";
                string result = name.Substring(name.IndexOf(split) + split.Length);
                return EscapeValue(result?.Trim());
            }
            return string.Empty;
        }

        private static string EscapeValue(string value)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char ch in value)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_' || ch == '-')
                {
                    sb.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    sb.Append('_');
                }
            }

            return sb.ToString();
        }
    }
}
