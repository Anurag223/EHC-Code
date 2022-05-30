using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using Moq;
using TLM.EHC.Admin;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using TLM.EHC.ADMIN.API.Controllers;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Querying;
using System.Linq;
using FluentAssertions;
using TLM.EHC.Common.Historian;
using TLM.EHC.ADMIN.API.Services;
using Tlm.Sdk.Testing.Unit;

namespace EHC.ADMIN.API.Tests.Controller
{
    [UnitTestCategory]
    [TestClass]
    public class AdminApiImplementationTest
    {
        private MockRepository _mockProvider;
        private Mock<IEpicV3HierarchyProvider> _mockEpicV3HierarchyProvider;
        private Mock<IInfluxDBMappingService> _mockInfluxDbMappingService;
        private Mock<IDBMapConflictLogService> _mockDbMapConflictLogService;
        private Mock<IA2RUtilsAuditLogService> _mockA2RUtilsAuditLogService;
        private Mock<IHistorianClient> _mockHistorianClient;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockInfluxDbMappingService = _mockProvider.Create<IInfluxDBMappingService>();
            _mockEpicV3HierarchyProvider = _mockProvider.Create<IEpicV3HierarchyProvider>();
            _mockDbMapConflictLogService = _mockProvider.Create<IDBMapConflictLogService>();
            _mockA2RUtilsAuditLogService = _mockProvider.Create<IA2RUtilsAuditLogService>();
            _mockHistorianClient = _mockProvider.Create<IHistorianClient>();

        }

        [TestMethod]
        public async Task Verify_GetAllInfluxDBMappingData_Returns_Correct_Count()
        {

            List<InfluxDBMapping> list = new List<InfluxDBMapping>()
            {
                new InfluxDBMapping(){DbName="test1",EquipmentCodes = new List<string>()
                {
                    "SPF-743"
                } ,Status=InfluxDBStatus.Enabled},
                new InfluxDBMapping(){DbName="test2",EquipmentCodes = new List<string>()
                {
                    "SPF-783"
                } ,Status=InfluxDBStatus.Enabled},
            };
            CollectionResult<InfluxDBMapping> collectionResult = new CollectionResult<InfluxDBMapping>(list);
            _mockInfluxDbMappingService.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult(collectionResult));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            var result = await apiImplementation.GetAllInfluxDBMappingData(QuerySpec.ForEverything);
            Assert.AreEqual(result.Collection.Count, list.Count);
        }

        [TestMethod]
        public async Task Verify_GetAllInfluxDBMappingData_Returns_NoData()
        {
            _mockInfluxDbMappingService.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult((CollectionResult<InfluxDBMapping>) null));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            var result = await apiImplementation.GetAllInfluxDBMappingData(QuerySpec.ForEverything);
            Assert.AreEqual(result, null);
        }

        [TestMethod]
        public async Task Verify_SetInfluxDbMappingStatus_Returns_True()
        {
            _mockInfluxDbMappingService.Setup(o => o.SetInfluxDbMappingStatus(It.IsAny<string>(), true))
           .Returns(Task.FromResult(true));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            var result = await apiImplementation.SetInfluxDbMappingStatus("SPF-343", true);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public async Task Verify_CreateDbInInflux_HappyPath()
        {
            InfluxDBMapping mapping = new InfluxDBMapping()
            {
                DbName = "test1",
                EquipmentCodes = new List<string>()
                {
                    "SPF-743"
                },
                Status = InfluxDBStatus.Enabled
            };
            _mockInfluxDbMappingService.Setup(o => o.GetInfluxDBName(It.IsAny<string>()))
                .Returns(Task.FromResult(mapping));
            _mockHistorianClient.Setup(o => o.CreateDatabase(mapping.DbName))
                .Returns(Task.FromResult(It.IsAny<QueryResult>()));
            AdminApiImplementation apiImplementation = GetApiImplementationInstance();
            //Act
            await apiImplementation.CreateDbInInflux("SPF-743");
            mapping.DbName.Should().BeOfType<string>();
        }

        [TestMethod]
        public async Task Verify_CreateUpdateDbMap_HappyPath()
        {
            string epicV3Wkid = "7:e_code01";
            EquipmentModel testModel = new EquipmentModel()
            {
                BrandCode = "1",
                BrandName = "BrandOne",
                Channels = new List<EquipmentModelChannel>()
                    {
                        new EquipmentModelChannel(){ Code="Code1", Dimension = "Dimension1", LegalClassification = "LegalClassification1", Name="Name 1", Uom="Uom1"}
                    },
                Description = "Description1",
                MaterialNumber = "1",
                EquipmentCode = "e_code01",
                TechnologyCode = "Tech1",
                TechnologyName = "TechName1"
            };

            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();

            _mockEpicV3HierarchyProvider.Setup(o => o.GetEpicHierarchyInfoFromEquipmentCode(epicV3Wkid, "e_code01"))
         .Returns(Task.FromResult(testModel));
            _mockInfluxDbMappingService.Setup(o => o.CreateUpdateDBMapping(It.IsAny<InfluxDBMapping>()))
           .Returns(Task.FromResult(mappingResponse));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.CreateUpdateDbMap(testModel.EquipmentCode);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(InfluxMappingResponse));
        }

        [TestMethod]
        public async Task Verify_GetConflictStatusByEquipmentCode_HappyPath()
        {
            List<InfluxDBMapping> dummyMaps = new List<InfluxDBMapping>()
            {
                new InfluxDBMapping(){DbName="test1",EquipmentCodes = new List<string>()
                {
                    "SPF-743"
                } ,Status=InfluxDBStatus.Enabled},
                new InfluxDBMapping(){DbName="test2",EquipmentCodes = new List<string>()
                {
                    "SPF-783"
                } ,Status=InfluxDBStatus.Enabled},
            };
            CollectionResult<InfluxDBMapping> mapsCollection = new CollectionResult<InfluxDBMapping>(dummyMaps);
            List<string> equipmentCodes = mapsCollection.Collection.SelectMany(o => o.EquipmentCodes).ToList();

            _mockDbMapConflictLogService.Setup(o => o.GetConflictStatusByEquipmentCode(equipmentCodes[0], false))
         .Returns(Task.FromResult(DBMapConflictStatus.OutOfSync));
            _mockDbMapConflictLogService.Setup(o => o.GetConflictStatusByEquipmentCode(equipmentCodes[1], false))
       .Returns(Task.FromResult(DBMapConflictStatus.InSync));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.GetConflictStatusByEquipmentCode(mapsCollection);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Keys.Count, 2);
            Assert.IsInstanceOfType(result, typeof(Dictionary<string, DBMapConflictStatus>));

        }

        [TestMethod]
        public async Task Verify_GetAllEpicDBMapConflictLog_Returns_Distinct_EquipmentCode()
        {
            List<EpicDBMapConflictLog> list = new List<EpicDBMapConflictLog>()
            {
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T1",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow.AddDays(-1)
                },
                 new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T2",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
                },
               new EpicDBMapConflictLog(){
                   DBMapEquipmentCode="Eq2",
                    DBMapBrandCode="BC3",
                    EpicBrandCode ="BC4",
                    DBMapTechnologyCode="T3",
                    EpicTechnologyName="T4",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
               },
            };
            CollectionResult<EpicDBMapConflictLog> collectionResult = new CollectionResult<EpicDBMapConflictLog>(list);
            _mockDbMapConflictLogService.Setup(o => o.GetAllEpicDBMapConflictLogByCriteria(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult(collectionResult));
            _mockDbMapConflictLogService.Setup(x => x.GetAllEpicDBMapConflictLogs()).Returns(Task.FromResult(list));
            AdminApiImplementation apiImplementation = GetApiImplementationInstance();
            //Act
            var result = await apiImplementation.GetAllEpicDBMapConflictLog(QuerySpec.ForEverything);
            var distinctDataForEq1 = result.Collection.Where(o => o.DBMapEquipmentCode == "Eq1").ToList();
            Assert.AreEqual(1, distinctDataForEq1.Count);
        }

        [TestMethod]
        public async Task Verify_GetAllEpicDBMapConflictLog_For_Correct_ConflictStartDate()
        {
            List<EpicDBMapConflictLog> list = new List<EpicDBMapConflictLog>()
            {
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T1",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow.AddDays(-1)
                },
                 new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T2",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
                },
               new EpicDBMapConflictLog(){
                   DBMapEquipmentCode="Eq2",
                    DBMapBrandCode="BC3",
                    EpicBrandCode ="BC4",
                    DBMapTechnologyCode="T3",
                    EpicTechnologyName="T4",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
               },
            };
            CollectionResult<EpicDBMapConflictLog> collectionResult = new CollectionResult<EpicDBMapConflictLog>(list);
            _mockDbMapConflictLogService.Setup(o => o.GetAllEpicDBMapConflictLogByCriteria(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult(collectionResult));
            _mockDbMapConflictLogService.Setup(x => x.GetAllEpicDBMapConflictLogs()).Returns(Task.FromResult(list));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();
            //Act
            var result = await apiImplementation.GetAllEpicDBMapConflictLog(QuerySpec.ForEverything);
            var dataToAssert = result.Collection.Where(o => o.DBMapEquipmentCode == "Eq1").ToList();
            Assert.AreEqual(list[0].CreatedDate.ToString(CultureInfo.CurrentCulture), dataToAssert[0].ConflictStartDate);
        }
               
        [TestMethod]
        public async Task Verify_GetA2RUtilsAuditLog_Returns_Correct_Count()
        {

            List<A2RUtilsAuditLog> list = new List<A2RUtilsAuditLog>()
            {
                new A2RUtilsAuditLog()
                {
                    ActivityType= A2RUtilsActivityType.AddEquipmentCode,
                    ApplicationName= A2RUtilsApplicationType.DbMapManagement,
                    OldValue= string.Empty,
                    NewValue= "SPF-745",

                },
                new A2RUtilsAuditLog()
                {
                    ActivityType= A2RUtilsActivityType.UpdateDbMapStatus,
                    ApplicationName= A2RUtilsApplicationType.DbMapManagement,
                    OldValue= "Disabled",
                    NewValue= "Enabled",
                },
            };
            CollectionResult<A2RUtilsAuditLog> collectionResult = new CollectionResult<A2RUtilsAuditLog>(list);
            _mockA2RUtilsAuditLogService.Setup(o => o.GetAllA2RUtilsAuditLog(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult(collectionResult));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            var result = await apiImplementation.GetA2RUtilsAuditLog(QuerySpec.ForEverything);
            _mockA2RUtilsAuditLogService.VerifyAll();
            Assert.AreEqual(result.Collection.Count, list.Count);
        }

        [TestMethod]
        public async Task Verify_GetA2RUtilsAuditLog_Returns_NoData()
        {
            _mockA2RUtilsAuditLogService.Setup(o => o.GetAllA2RUtilsAuditLog(It.IsAny<QuerySpec>()))
           .Returns(Task.FromResult((CollectionResult<A2RUtilsAuditLog>) null));

            AdminApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            var result = await apiImplementation.GetA2RUtilsAuditLog(QuerySpec.ForEverything);
            _mockA2RUtilsAuditLogService.VerifyAll();
            Assert.AreEqual(result, null);
        }

        private AdminApiImplementation GetApiImplementationInstance()
        {
            return new AdminApiImplementation(_mockEpicV3HierarchyProvider.Object,
                _mockInfluxDbMappingService.Object, _mockDbMapConflictLogService.Object,
                _mockA2RUtilsAuditLogService.Object, _mockHistorianClient.Object);
        }


        [TestCleanup]
        public void ResetMockObjectState()
        {
            _mockEpicV3HierarchyProvider = null;
            _mockInfluxDbMappingService = null;
            _mockDbMapConflictLogService = null;
            _mockA2RUtilsAuditLogService = null;
        }
    }
}
