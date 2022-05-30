using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.HealthCheck;
using TLM.EHC.Common.Clients.EquipmentModelApi;
using EquipmentModel = TLM.EHC.Common.Clients.EquipmentModelApi.EquipmentModel;

namespace EHC.API.Tests.HealthCheckTest
{
    [UnitTestCategory]
    [TestClass]
    public class HealthCheckEquipmentModelApiTest
    {
        private MockRepository _mockProvider;
        private Mock<IEquipmentModelApiClient> _mockEquipmentModelApiClient;
        private HealthCheckEquipmentModelApi _healthCheckObj;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockEquipmentModelApiClient = _mockProvider.Create<IEquipmentModelApiClient>();
            _healthCheckObj = new HealthCheckEquipmentModelApi(_mockEquipmentModelApiClient.Object);
        }

        [TestMethod]
        public async Task VerifyEquipmentModelApiHealthyConnection()
        {
            _mockEquipmentModelApiClient.Setup(o => o.GetByEquipmentCodeAsync("TCS-323",null))
                .Returns(Task.FromResult(new EquipmentModel() { Description = "test description" }));
            var result = await _healthCheckObj.CheckHealthAsync(new HealthCheckContext());
            result.Status.Should().Be(HealthStatus.Healthy);

        }
    }
}
