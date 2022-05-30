using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.Common.Clients.EquipmentApi;
using TLM.EHC.API.HealthCheck;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.HealthCheckTest
{
    [UnitTestCategory]
    [TestClass]
    public class HealthCheckEquipmentApiTest
    {
        private MockRepository _mockProvider;
        private Mock<IEquipmentApiClient> _mockEquipmentApiClient;
        private HealthCheckEquipmentApi _healthCheckObj;
       
       [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockEquipmentApiClient = _mockProvider.Create<IEquipmentApiClient>();
            _healthCheckObj = new HealthCheckEquipmentApi(_mockEquipmentApiClient.Object);
        }

        [TestMethod]
        public async Task VerifyEquipmentApiHealthyConnection()
        {
            _mockEquipmentApiClient.Setup(o => o.GetEquipmentByWkeId("100298911:TCS32300Y0423"))
                .Returns(Task.FromResult(new Equipment() { Description = "test description"}));
           var result= await _healthCheckObj.CheckHealthAsync(new HealthCheckContext());
           result.Status.Should().Be(HealthStatus.Healthy);

        }
       
    }
}
