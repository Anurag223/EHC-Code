using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.HealthCheck;
using TLM.EHC.Common.Historian;

namespace EHC.API.Tests.HealthCheckTest
{
    [UnitTestCategory]
    [TestClass]
    public class HealthCheckInfluxDbTest
    {
        private MockRepository _mockProvider;
        private Mock<IHistorianClient> _mockHistorianClient;
        private HealthCheckInfluxDb _healthCheckObj;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockHistorianClient = _mockProvider.Create<IHistorianClient>();
            _healthCheckObj = new HealthCheckInfluxDb(_mockHistorianClient.Object);
        }

        [TestMethod]
        public async Task VerifyInfluxDbHealthyConnection()
        {
            QueryResult queryResult = new QueryResult(){ Values=new List<List<object>>()};
            _mockHistorianClient.Setup(o => o.ShowDatabases())
                .Returns(Task.FromResult(queryResult));
            var result = await _healthCheckObj.CheckHealthAsync(new HealthCheckContext());
            result.Status.Should().Be(HealthStatus.Healthy);

        }
    }
}
