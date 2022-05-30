using System.Threading.Tasks;
using EHC.API.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Historian;
using EHC.API.Tests.Mocks;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class EpisodicPointServiceTest
    {
        private MockRepository _mockProvider;
        Mock<IHistorianClient> _mockHistorainProvider;
        Mock<IEquipmentProvider> _mockEquipmentProvider;
        Mock<IEquipmentModelProvider> _mockEquipmentModelProvider;
        private IEpisodicPointService _service;
        private Equipment _testEquipment;
        private EquipmentModel equipmentModel;
        private QueryResult queryResult;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockHistorainProvider = _mockProvider.Create<IHistorianClient>();
             _mockEquipmentProvider = _mockProvider.Create<IEquipmentProvider>();
             _mockEquipmentModelProvider = _mockProvider.Create<IEquipmentModelProvider>();
             _testEquipment = TestData.TestDataEquipment();
            equipmentModel = TestData.TestDataEquipmentModel();
            queryResult = TestData.TestDataQueryResult();
        }

        [TestMethod]
        public void Verify_AnyPointExists_ReturnsTrue()
        {
            _mockEquipmentProvider =
               new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(null ,
                   _testEquipment);

            _mockEquipmentModelProvider =
               new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCode(
                   null, equipmentModel);

            _mockHistorainProvider = new MockHistorianClient().ConfigureMockForPerformQuery(queryResult);

            _service = new EpisodicPointService(_mockHistorainProvider.Object, _mockEquipmentProvider.Object, _mockEquipmentModelProvider.Object);

            var result = _service.AnyPointsExists("12345678", "1234:23456");
            Assert.IsTrue(result.Result);

        }
       
        [TestMethod]
        public void Verify_AnyPointExists_ReturnsFalse_WhenEquipmentNotFound()
        {
            _mockEquipmentProvider.Setup(o => o.GetEquipmentByWkeid(It.IsAny<WellKnownEntityId>())).Returns(Task.FromResult((Equipment) null));

            _service = new EpisodicPointService(_mockHistorainProvider.Object, _mockEquipmentProvider.Object, _mockEquipmentModelProvider.Object);

            var result = _service.AnyPointsExists("12345678", "1234:23456");
            Assert.IsFalse(result.Result);
        }

        [TestMethod]
        public void Verify_AnyPointExists_ReturnsFalse_WhenPerformQueryUnsuccessful()
        {
            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(null,
                    _testEquipment);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCode(
                    null, equipmentModel);

            _mockHistorainProvider = new MockHistorianClient().ConfigureMockForPerformQuery(null);

            _service = new EpisodicPointService(_mockHistorainProvider.Object, _mockEquipmentProvider.Object, _mockEquipmentModelProvider.Object);

            var result = _service.AnyPointsExists("12345678", "1234:23456");
            Assert.IsFalse(result.Result);

        }
    }
}
