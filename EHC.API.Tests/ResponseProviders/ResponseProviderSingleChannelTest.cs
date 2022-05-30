using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ResponseProviders
{
    [UnitTestCategory]
    [TestClass]
    public class ResponseProviderSingleChannelTest
    {
        private MockRepository _mockProvider;
        Mock<IHistorianClient> _mockHistorianClient;
        Mock<IChannelDefinitionService> _mockChannelDefinitionService;
        private ResponseProviderSingleChannel _responseChannel;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockHistorianClient = _mockProvider.Create<IHistorianClient>();
            _mockChannelDefinitionService = _mockProvider.Create<IChannelDefinitionService>();
            _responseChannel = new ResponseProviderSingleChannel(_mockHistorianClient.Object,
                _mockChannelDefinitionService.Object);
        }

        [TestMethod]
        public void Verify_GetResponse_Successful()
        {
            var dummyEquipment = new Equipment()
            {
                EquipmentCode = "SPF-743",
                EquipmentWkeId = "100949474:1396533",
                MaterialNumber = "100949474",
                SerialNumber = "1396533",
                SourceSystemRecordId = "1396533"
            };

            var query = new Query("DXP_WPS_PUMPING_EQUIPMENT",
                "SELECT * FROM STIMULATION_PUMP_WS - 67 WHERE EquipmentInstance='100949474:1396533' AND time >= 1564327807293000000 AND time <= 1564414207293000000");
            var queryContext = new QueryContext()
            {
                Equipment = dummyEquipment
            };

            QueryResult queryResult = new QueryResult
            {
                Values = new List<List<object>>() {new List<object>() { new DateTime(2021,01,02)
                    , new DateTime(2021,01,02) } },
                Columns = new List<string>(){"test","test2"}
            };

            ChannelDefinitionClean cl = new ChannelDefinitionClean()
            {
                Code = "testCode",
                Uom = "testUom",
                Dimension = "testDimension",
                LegalClassification = "legalClassification",
                Type = "Channel"
            };

            _mockHistorianClient.Setup(o => o.PerformQuery(query))
                .Returns(Task.FromResult(queryResult));

            _mockChannelDefinitionService.Setup(o => o.GetChannelDescription("test")).Returns(Task.FromResult(cl));

           var result = _responseChannel.GetResponse(query, queryContext);
           result.Result.Should().BeOfType<ApiResponse>();

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public void Verify_GetResponseNoData_ForException()
        {
            _responseChannel.GetResponseNoData(new RowsRequest() {Codes = new string[] {"test1", "test2"}}, null);
        }
    }
}
