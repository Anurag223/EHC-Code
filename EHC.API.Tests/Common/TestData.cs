using System.Collections.Generic;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;

namespace EHC.API.Tests.Common
{
    public class TestData
    {
        public static QueryResult TestDataQueryResult()
        {
            return new QueryResult()
            {
                Name = "TestQuery",
                Columns = new List<string>(),
                Values = new List<List<object>>()
            };
        }

        public static EquipmentModel TestDataEquipmentModel()
        {
            return new EquipmentModel()
            {
                BrandCode = "brandcode",
                BrandName = "brandname",
                Channels = new List<EquipmentModelChannel>(),
                Description = "testdescription",
                EquipmentCode = "testequipmentcode",
                MaterialNumber = "materialnumber",
                TechnologyCode = "technologycode",
                TechnologyName = "technologyname"
            };
        }

        public static Equipment TestDataEquipment()
        {
            return new Equipment()
            {
                EquipmentCode = "testequipmentcode",
                EquipmentWkeId = "testequipmentwkeid",
                MaterialNumber = "testmaterialnumber",
                SerialNumber = "testserialnumber",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<TLM.EHC.Common.Clients.EquipmentApi.Classification>() {
                    new TLM.EHC.Common.Clients.EquipmentApi.Classification
                    {
                        Code="code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new TLM.EHC.Common.Clients.EquipmentApi.Classification{
                        Code="code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new TLM.EHC.Common.Clients.EquipmentApi.Classification{
                        Code="code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };
        }
    }
}