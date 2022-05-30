using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.ADMIN.API.Controllers;
using TLM.EHC.Common.Exceptions;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Testing.Unit;

namespace EHC.ADMIN.API.Tests.Controller
{
    [UnitTestCategory]
    [TestClass]
    public class A2RUtilsAuditLogControllerTest
    {
        private MockRepository _mockProvider;
        private A2RUtilsAuditLogController _controller;
        private Mock<IAdminApiImplementation> _adminApiImplementation;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _adminApiImplementation = _mockProvider.Create<IAdminApiImplementation>();
            _controller = new A2RUtilsAuditLogController(_adminApiImplementation.Object);

        }

        private List<A2RUtilsAuditLog> mockA2RAuditLogs = new List<A2RUtilsAuditLog>()
            {
                new A2RUtilsAuditLog()
                {
                    ActivityType = A2RUtilsActivityType.AddEquipmentCode,
                    ApplicationName = A2RUtilsApplicationType.DbMapManagement,
                    OldValue = string.Empty,
                    NewValue = "SPF-745",

                },
                new A2RUtilsAuditLog()
                {
                    ActivityType = A2RUtilsActivityType.UpdateDbMapStatus,
                    ApplicationName = A2RUtilsApplicationType.DbMapManagement,
                    OldValue = "Disabled",
                    NewValue = "Enabled",
                }
            };

        private JToken Get_Dummy_Json()
        {
            return JToken.Parse(@"
                {
""createdDate"": ""2021-12-20T09:41:04.445Z"",
  ""createdBy"": ""ABC"",
  ""modifiedDate"": ""2021-12-20T09:41:04.445Z"",
  ""modifiedBy"": ""ABC"",
 ""activityType"": ""AddEquipmentCode"",
  ""applicationName"": ""DbMapManagement"",
  ""oldValue"": ""x"",
  ""newValue"": ""y""
}"
               );
        }

        [TestMethod]
        public async Task Verify_GetA2RUtilsAuditLogs_HappyPath()
        {
            var logs = new CollectionResult<A2RUtilsAuditLog>(mockA2RAuditLogs);
            _adminApiImplementation.Setup(o => o.GetA2RUtilsAuditLog(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(logs));
            //Act
            var result = await _controller.GetA2RUtilsAuditLog(QuerySpec.ForEverything);
            //Assert
            Assert.IsNotNull(result);
            var responseObject = (OkObjectResult)result;
            var responseBatch = responseObject.Value as List<A2RUtilsAuditLog>;
            Assert.IsTrue(responseBatch != null && responseBatch.Count == 2);

        }

        [TestMethod]
        public void Verify_GetA2RUtilsAuditLogs_ForNoContent()
        {
            _adminApiImplementation.Setup(o => o.GetA2RUtilsAuditLog(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult((CollectionResult<A2RUtilsAuditLog>)null));

            var result = _controller.GetA2RUtilsAuditLog(QuerySpec.ForEverything);
            Assert.IsInstanceOfType(result.Result, typeof(NoContentResult));

        }

        [TestMethod]
        public async Task Verify_CreateA2RUtilsAuditLog_ForSuccess()
        {
            var inputJson = Get_Dummy_Json();
            A2RUtilsAuditLog log = inputJson.ToObject<A2RUtilsAuditLog>();
            _adminApiImplementation.Setup(o => o.CreateA2RUtilsAuditLog(log)).
                Returns(Task.FromResult(log));
            var result = await _controller.CreateA2RUtilsAuditLog(log);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsInstanceOfType(((ObjectResult)result).Value, typeof(A2RUtilsAuditLog));
        }

        [TestMethod]
        public async Task Verify_CreateA2RUtilsAuditLog_Throws_InternalServerError()
        {
            var inputJson = Get_Dummy_Json();
            string errorMessage = "Error occurred";
            A2RUtilsAuditLog log = inputJson.ToObject<A2RUtilsAuditLog>();
            _adminApiImplementation.Setup(o => o.CreateA2RUtilsAuditLog(log))
                .Throws(new ServerErrorException(errorMessage));
            var result = await _controller.CreateA2RUtilsAuditLog(log);
            result.Should().NotBeNull();
            Assert.AreEqual(StatusCodes.Status500InternalServerError, ((ObjectResult)(result)).StatusCode);
            Assert.AreEqual(errorMessage,((ObjectResult)result).Value);
        }


    }
}



