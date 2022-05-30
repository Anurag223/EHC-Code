using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.Admin;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.ADMIN.API.Services;
using System;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Testing.Unit;

namespace EHC.ADMIN.API.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class A2RUtilsAuditLogServiceTests
    {
        private MockRepository _mockProvider;
        Mock<IRepositoryHandler<A2RUtilsAuditLog>> _mockRepositoryHandler;
        private A2RUtilsAuditLogService _service;


        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockRepositoryHandler = _mockProvider.Create<IRepositoryHandler<A2RUtilsAuditLog>>();
            _service = new A2RUtilsAuditLogService(_mockRepositoryHandler.Object);
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
                    Id="testid2",
                    ActivityType = A2RUtilsActivityType.UpdateDbMapStatus,
                    ApplicationName = A2RUtilsApplicationType.DbMapManagement,
                    OldValue = "Disabled",
                    NewValue = "Enabled",
                }
            };

        [TestMethod]
        public async Task Test_GetAllA2RUtilsAuditLog_ReturnNull()
        {
            mockA2RAuditLogs.Clear();
            CollectionResult<A2RUtilsAuditLog> coll = new CollectionResult<A2RUtilsAuditLog>(mockA2RAuditLogs);
            _mockRepositoryHandler.Setup(x => x.QueryManyAsync(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(coll));

            var result = await _service.GetAllA2RUtilsAuditLog(QuerySpec.ForEverything);
            result.Should().BeNull();
        }

        [TestMethod]
        public async Task Test_GetAllA2RUtilsAuditLog_Retuns_Correct_Count()
        {
            CollectionResult<A2RUtilsAuditLog> coll = new CollectionResult<A2RUtilsAuditLog>(mockA2RAuditLogs);
            _mockRepositoryHandler.Setup(x => x.QueryManyAsync(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(coll));

            var result = await _service.GetAllA2RUtilsAuditLog(QuerySpec.ForEverything);
            Assert.That.Should().NotBeNull();
            Assert.AreEqual(result.Collection.Count, mockA2RAuditLogs.Count);
        }      

        [TestMethod]
        public void Test_CreateA2RUtilsAuditLog_WhenIdNotNull_ThrowsException()
        {           
            var result = _service.CreateA2RUtilsAuditLog(mockA2RAuditLogs[1]);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be(EhcConstants.IdShouldBeNull);
        }

    }
}



