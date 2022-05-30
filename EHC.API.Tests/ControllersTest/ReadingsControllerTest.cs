using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client.Rows;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class ReadingsControllerTest
    {
        private Mock<IDataParser> _mockDataParser;
        private Mock<IDataMapper> _mockDataMapper;
        private Mock<IApiImplementation> _mockApiImplementation;
        private Mock<IHistorianClient> _mockHistorianClient;
        private static TimestampParser _mockTimeStampParser;
        private ReadingsController _mockReadingsController;
        private JToken _dummyinputJson;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockDataMapper = new Mock<IDataMapper>();
            _mockDataParser = new Mock<IDataParser>();
            _mockApiImplementation = new Mock<IApiImplementation>();
            _mockHistorianClient = new Mock<IHistorianClient>();
            _mockTimeStampParser = new TimestampParser();
            _mockReadingsController = GetMockReadingsController();
            _dummyinputJson = Get_Dummy_Json();
        }

        protected ReadingsController GetMockReadingsController()
        {
            return new ReadingsController(_mockApiImplementation.Object, _mockDataParser.Object, _mockDataMapper.Object,
                null, _mockHistorianClient.Object);
        }


        [TestMethod]
        public void Get_Should_Be_Decorated()
        {
            ApiBaseTest.ValidateGetMethodsWithBadRequestAttributes<ReadingsController>();
            ApiBaseTest.ValidateGetMethodsWithProduceOkAttributes<ChannelDefinition, ReadingsController>();
        }

        [TestMethod]
        public void Test_PostReading_WithOkResponse()
        {
            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(),
                    It.IsAny<DynamicInfluxRow[]>(),
                    It.IsAny<DataType>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IActionResult>(new OkResult()));

            var result = _mockReadingsController.PostReadings("100949474:SPF74312A0123", _dummyinputJson.ToObject<MultipleChannelsRequest>());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            _mockDataParser.Verify(o => o.ParseChannelsData(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsData(It.IsAny<JArray>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRows(It.IsAny<ParsedRows>(), It.IsAny<ChannelDefinitionIndex[]>()),
                Times.Once);

        }

        [TestMethod]
        public void Test_PostReading_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(), 
                                                                It.IsAny<DynamicInfluxRow[]>(),
                                                                It.IsAny<DataType>(), It.IsAny<string>()))
                                  .Throws(new BadRequestException("Error"));

            var result = _mockReadingsController.PostReadings("100949474:SPF74312A0123", _dummyinputJson.ToObject<MultipleChannelsRequest>());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            _mockDataParser.Verify(o => o.ParseChannelsData(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsData(It.IsAny<JArray>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRows(It.IsAny<ParsedRows>(), It.IsAny<ChannelDefinitionIndex[]>()),
                Times.Once);

        }

        [TestMethod]
        public void Test_GetReadings()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockReadingsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));
            var result = _mockReadingsController.GetReadings("100949474:SPF74312A0123", null, null, "time,code");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Test_GetReadings_ByCode()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockReadingsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));
            var result = _mockReadingsController.GetReadingByCode("100949474:SPF74312A0123", "time,code", null, null);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Test_GetReadings_ForException()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockReadingsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>()))
                .Throws(new NotFoundException("Error"));
            var result = _mockReadingsController.GetReadingByCode("100949474:SPF74312A0123", "time,code", null, null);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void Test_GetChannelDefinition_WithOkResponse()
        {
            var output = new ChannelDefinitionClean[0];
            var objectResult = new OkObjectResult(output);
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(),
                    It.IsAny<DataType>()))
                .Returns(Task.FromResult(output));
            var result = _mockReadingsController.GetReadingDefinitions("100949474:SPF74312A0123");
            (result.Result).Should().NotBeNull();
            Assert.AreEqual((result.Result as OkObjectResult).StatusCode, 200);
        }

        [TestMethod]
        public void Test_GetChannelDefinition_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(),
                    It.IsAny<DataType>()))
                .Throws(new BadRequestException("Error"));
            var result = _mockReadingsController.GetReadingDefinitions("100949474:SPF74312A0123");
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        private JToken Get_Dummy_Json()
        {
            return JToken.Parse(
                "\r\n{\r\n  \"meta\": {\r\n    \"channels\": [\r\n      {\r\n        \"code\": \"time\",\r\n        \"uom\": \"d\",\r\n        \"dimension\": \"time\"\r\n      },\r\n      {\r\n        \"code\": \"AirPressure\",\r\n        \"uom\": \"kPa\",\r\n        \"dimension\": \"pressure\"\r\n      },\r\n      {\r\n        \"code\": \"DischargePressure\",\r\n        \"uom\": \"kPa\",\r\n        \"dimension\": \"pressure\"\r\n      },\r\n      {\r\n        \"code\": \"DischargeRate\",\r\n        \"uom\": \"unitless\",\r\n        \"dimension\": \"ratio\"\r\n      }\r\n    ] \r\n  },\r\n  \"rows\": [\r\n    [\r\n      \"2019-07-29T15:30:02.156Z\",\r\n      655829.25,\r\n      null,\r\n      null\r\n    ],\r\n    [\r\n      \"2019-07-29T15:30:04.431Z\",\r\n      655830.84,\r\n      491541.375,\r\n      0.0870783925056458\r\n    ],\r\n    [\r\n      \"2019-07-29T15:30:07.293Z\",\r\n      null,\r\n      null,\r\n      0.0871633142232895\r\n    ]\r\n  ]\r\n}\r\n");
        }





    }
}
