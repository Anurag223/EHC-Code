using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.ADMIN.API.ControllerModels;
using TLM.EHC.ADMIN.API.Controllers;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Testing.Unit;

namespace EHC.ADMIN.API.Tests.Controller
{
    [UnitTestCategory]
    [TestClass]
    public class InfluxDbMappingControllerTest
    {
        private MockRepository _mockProvider;
        private InfluxDbMappingController _controller;
        private Mock<IAdminApiImplementation> _adminApiImplementation;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _adminApiImplementation = _mockProvider.Create<IAdminApiImplementation>();
            _controller = new InfluxDbMappingController(_adminApiImplementation.Object);

        }

        [TestMethod]
        public void Verify_GetDbMapDetails_ForSuccessful()
        {
            List<InfluxDBMapping> list = new List<InfluxDBMapping>()
            {
                new InfluxDBMapping()
                {
                    DbName = "test1", EquipmentCodes = new List<string>()
                    {
                        "SPF-743"
                    },
                    Status = InfluxDBStatus.Enabled
                },
                new InfluxDBMapping()
                {
                    DbName = "test2", EquipmentCodes = new List<string>()
                    {
                        "SPF-783"
                    },
                    Status = InfluxDBStatus.Enabled
                },
            };
            Dictionary<string, DBMapConflictStatus> conflictStatus = new Dictionary<string, DBMapConflictStatus>();
            conflictStatus.Add("SPF-743", DBMapConflictStatus.OutOfSync);
            conflictStatus.Add("SPF-783", DBMapConflictStatus.InSync);

            CollectionResult<InfluxDBMapping> collectionResult = new CollectionResult<InfluxDBMapping>(list);

            _adminApiImplementation.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(collectionResult));
            _adminApiImplementation.Setup(o => o.GetConflictStatusByEquipmentCode(collectionResult))
                .Returns(Task.FromResult(conflictStatus));

            var result = _controller.GetDbMapDetails(QuerySpec.ForEverything);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Verify_GetDbMapDetails_ForNoContent()
        {
            _adminApiImplementation.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult((CollectionResult<InfluxDBMapping>)null));

            var result = _controller.GetDbMapDetails(QuerySpec.ForEverything);
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));

        }

        [TestMethod]
        public void Verify_GetDbMapDetails_ForBadRequest()
        {
            List<InfluxDBMapping> list = new List<InfluxDBMapping>()
            {
                new InfluxDBMapping() {DbName = "test1", Status = InfluxDBStatus.Enabled},
                new InfluxDBMapping() {DbName = "test2", Status = InfluxDBStatus.Enabled},
            };

            CollectionResult<InfluxDBMapping> collectionResult = new CollectionResult<InfluxDBMapping>(list);

            _adminApiImplementation.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(collectionResult));

            var result = _controller.GetDbMapDetails(QuerySpec.ForEverything);
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public async Task Verify_GetInfluxDbMapping_HappyPath()
        {
            List<InfluxDBMapping> list = new List<InfluxDBMapping>()
            {
                new InfluxDBMapping()
                {
                    DbName = "test1", EquipmentCodes = new List<string>()
                    {
                        "SPF-743"
                    },
                    Status = InfluxDBStatus.Enabled
                },
                new InfluxDBMapping()
                {
                    DbName = "test2", EquipmentCodes = new List<string>()
                    {
                        "SPF-783"
                    },
                    Status = InfluxDBStatus.Enabled
                },
            };
            var mappingResult = new CollectionResult<InfluxDBMapping>(list);
            _adminApiImplementation.Setup(o => o.GetAllInfluxDBMappingData(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(mappingResult));
            var influxDbMappingController = new InfluxDbMappingController(_adminApiImplementation.Object);
            //Act
            var result = await influxDbMappingController.GetInfluxDbMapping(QuerySpec.ForEverything);
            //Assert
            Assert.IsNotNull(result);
            var responseObject = (OkObjectResult)result;
            var responseBatch = responseObject.Value as CollectionResult<InfluxDBMapping>;
            Assert.IsTrue(responseBatch != null && responseBatch.Collection.Count == 2);

        }

        [TestMethod]
        public async Task Verify_GetConflictLogs_HappyPath()
        {
            List<EpicDBMapConflictLog> list = new List<EpicDBMapConflictLog>()
            {
                new EpicDBMapConflictLog()
                {
                    DBMapEquipmentCode = "Eq1",
                    DBMapBrandCode = "BC1",
                    EpicBrandCode = "BC2",
                    DBMapTechnologyCode = "T1",
                    EpicTechnologyName = "T2",
                    ConflictStatus = DBMapConflictStatus.OutOfSync,
                    ConflictStartDate = "10/5/2021 10:00:03 AM"
                },
                new EpicDBMapConflictLog()
                {
                    DBMapEquipmentCode = "Eq2",
                    DBMapBrandCode = "BC3",
                    EpicBrandCode = "BC4",
                    DBMapTechnologyCode = "T3",
                    EpicTechnologyName = "T4",
                    ConflictStatus = DBMapConflictStatus.OutOfSync,
                    ConflictStartDate = "10/6/2021 10:10:03 AM"
                },
            };
            var mappingResult = new CollectionResult<EpicDBMapConflictLog>(list);
            _adminApiImplementation.Setup(o => o.GetAllEpicDBMapConflictLog(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(mappingResult));
            var influxDbMappingController = new InfluxDbMappingController(_adminApiImplementation.Object);
            //Act
            var result = await influxDbMappingController.GetEpicDBMapConflictLog(QuerySpec.ForEverything);
            //Assert
            Assert.IsNotNull(result);
            var responseObject = (OkObjectResult)result;
            var responseBatch = responseObject.Value as CollectionResult<EpicDBMapConflictLog>;
            Assert.IsTrue(responseBatch != null && responseBatch.Collection.Count == 2);

        }

        [TestMethod]
        public void Verify_GetConflictLogs_ForNoContent()
        {
            _adminApiImplementation.Setup(o => o.GetAllEpicDBMapConflictLog(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult((CollectionResult<EpicDBMapConflictLog>)null));

            var result = _controller.GetEpicDBMapConflictLog(QuerySpec.ForEverything);
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));

        }

        [TestMethod]
        public void Verify_UpdateInfluxDbMappingStatus_ForEmptyEquipmentCode()
        {
            _adminApiImplementation.Setup(o => o.SetInfluxDbMappingStatus(It.IsAny<string>(), true))
                .Returns(Task.FromResult(false));
            var result = _controller.UpdateInfluxDBMappingStatus(It.IsAny<string>(), true);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void Verify_UpdateInfluxDbMappingStatus_ForEqCodeNotInDbMap()
        {
            string eqCode = "ABC";
            _adminApiImplementation.Setup(o =>
                    o.SetInfluxDbMappingStatus(eqCode, true))
                .Throws(new NotFoundException("EqCode not found"));
            var result = _controller.UpdateInfluxDBMappingStatus(eqCode, true);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void Verify_UpdateInfluxDbMappingStatus_Both_MapUpdated_DbCreated()
        {
            string eqCode = "ABC";
            _adminApiImplementation.Setup(o =>
                    o.SetInfluxDbMappingStatus(eqCode, true))
                .Returns(Task.FromResult(true));
            _adminApiImplementation.Setup(o => o.CreateDbInInflux(eqCode))
                .Returns(Task.FromResult<IActionResult>(new OkResult()));
            var result = _controller.UpdateInfluxDBMappingStatus(eqCode, true);
            result.Should().NotBeNull();
            var response = ((InfluxAndDbMappingUpdateResponse)((ObjectResult)((result).Result)).Value);
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
            Assert.IsInstanceOfType(response, typeof(InfluxAndDbMappingUpdateResponse));
            Assert.AreEqual(EhcConstants.DbMappingUpdatedSuccessMessage, response.DbMapUpdateMessage);
            Assert.AreEqual(true, response.DbMapUpdateStatus);
            Assert.AreEqual(EhcConstants.InfluxDbCreationSuccessMessage, response.InfluxDbCreationMessage);

        }

        [TestMethod]
        public void Verify_UpdateInfluxDbMappingStatus_InfluxDBCreation_Throws500_NoDBMapUpdated()
        {
            string eqCode = "ABC";
            string errorMessage = "Error occurred";
            _adminApiImplementation.Setup(o =>
                    o.SetInfluxDbMappingStatus(eqCode, true))
                .Returns(Task.FromResult(true));
            _adminApiImplementation.Setup(o => o.CreateDbInInflux(eqCode))
                .Throws(new ServerErrorException(errorMessage));
            var result = _controller.UpdateInfluxDBMappingStatus(eqCode, true);
            result.Should().NotBeNull();
            var response = ((InfluxAndDbMappingUpdateResponse)((ObjectResult)((result).Result)).Value);
            Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)(result).Result).StatusCode);
            Assert.AreEqual(EhcConstants.InfluxDbCreationErrorMessage, response.InfluxDbCreationMessage);
            Assert.AreEqual(EhcConstants.DbMappingUpdatedErrorMessage, response.DbMapUpdateMessage);
            Assert.AreEqual(false, response.DbMapUpdateStatus);
            Assert.AreEqual(errorMessage, response.ErrorDetails);

        }

        [TestMethod]
        public void Verify_UpdateInfluxDbMappingStatus_DBMapUpdate_Throws500_NoInfluxDBCreated()
        {
            string eqCode = "ABC";
            string errorMessage = "Error occurred";
            _adminApiImplementation.Setup(o =>
                    o.SetInfluxDbMappingStatus(eqCode, true))
                .Throws(new ServerErrorException(errorMessage));
            var result = _controller.UpdateInfluxDBMappingStatus(eqCode, true);
            result.Should().NotBeNull();
            var response = ((InfluxAndDbMappingUpdateResponse)((ObjectResult)((result).Result)).Value);
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)(result).Result).StatusCode);       
            Assert.AreEqual(EhcConstants.InfluxDbCreationErrorMessage, response.InfluxDbCreationMessage);
            Assert.AreEqual(EhcConstants.DbMappingUpdatedErrorMessage, response.DbMapUpdateMessage);
            Assert.AreEqual(false, response.DbMapUpdateStatus);
            Assert.AreEqual(errorMessage, response.ErrorDetails);
        }

    }
}



