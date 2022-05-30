using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Models;
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
    public class ChannelsControllerTest
    {
        private MockRepository _mockProvider;
        private Mock<IApiImplementation> _mockApiImplementation;
        private Mock<IDataParser> _mockDataParser;
        private Mock<IDataMapper> _mockDataMapper;
        private Mock<ITimestampParser> _mockTimeStampParser;
        private JToken _dummyinputJson;
        private ChannelsController _mockcontroller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockApiImplementation = _mockProvider.Create<IApiImplementation>();
            _mockDataParser = _mockProvider.Create<IDataParser>();
            _mockDataMapper = _mockProvider.Create<IDataMapper>();
            _mockTimeStampParser = _mockProvider.Create<ITimestampParser>();
            _mockcontroller = GetChannelsController();
            _dummyinputJson = Get_Dummy_Json();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockProvider = null;
            _mockApiImplementation = null;
            _mockDataParser = null;
            _mockDataMapper = null;
            _mockTimeStampParser = null;
            _dummyinputJson = null;
            _mockcontroller = null;
        }

        [TestMethod]
        public void Get_Should_Be_Decorated()
        {
            ApiBaseTest.ValidateGetMethodsWithBadRequestAttributes<ChannelsController>();
            ApiBaseTest.ValidateGetMethodsWithNotAcceptableAttributes<ChannelsController>();
        }

        #region ChannelsController::GetChannel Tests

        /// <summary>
        /// HTTP status code 200 test for GetChannels controller method.
        /// /// </summary>
        [TestMethod]
        public void Test_GetChannels_ForCorrectResponse()
        {
            ApiResponse response = new ApiResponse(new SingleChannel());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetChannels("100949474:SPF74312A0123", null!, null!, "time,code",null, "");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

        }

        [TestMethod]
        public void Test_GetChannels_ThrowsNotFoundException()
        {
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>()))
                                  .Throws(new NotFoundException("Error"));

            var result = _mockcontroller.GetChannels("100949474:SPF74312A0123", null!, null!, "time,code", null, "");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }
        #endregion

        #region GetCalculatedChannels Test
        [TestMethod]
        public void Test_GetCalculatedChannels_ForCorrectResponse()
        {
            var response = new ApiResponse(new MultipleChannels());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetCalculatedChannels("100949474:SPF74312A0123", "AirPressure", "DischargeRate", null!, null!, null!);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

        }

        [TestMethod]
        public void Test_GetCalculatedChannels_ForHttpStatusException()
        {
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Throws(new BadRequestException("Exception Message"));

            var result = _mockcontroller.GetCalculatedChannels("100949474:SPF74312A0123", "AirPressure", "DischargeRate", null!, null!, null!);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void Test_GetCalculatedChannels_WhenInvalidEquipmentWkeid()
        {
            var response = new ApiResponse(new MultipleChannels());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetCalculatedChannels("100949474", "AirPressure", "DischargeRate", null!, null!, null!);
            result.Result.Should().NotBeNull();
            ((Tlm.Sdk.Core.Models.Hypermedia.Error)((ObjectResult)result.Result).Value).Code.Should()
                .Be("INVALIDEQUIPMENTWKEID");
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public void Test_GetCalculatedChannels_WhenInvalidDateFormat()
        {
            var response = new ApiResponse(new MultipleChannels());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetCalculatedChannels("100949474:239","AirPressure", "DischargeRate", "2018/02/04", "2018/03/05", null!);
            result.Result.Should().NotBeNull();
            ((Tlm.Sdk.Core.Models.Hypermedia.Error)((ObjectResult)result.Result).Value).Code.Should()
                .Be("INVALIDSTARTDATE");
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public void Test_GetCalculatedChannels_WhenStartTimeIsPassedOnly()
        {
            var response = new ApiResponse(new MultipleChannels());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetCalculatedChannels("100949474:239", "AirPressure", "DischargeRate", "2018-04-06", null!, null!);
            result.Result.Should().NotBeNull();
            ((Tlm.Sdk.Core.Models.Hypermedia.Error)((ObjectResult)result.Result).Value).Code.Should()
                .Be("INVALIDSTARTDATE");
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public void Test_GetCalculatedChannels_WhenResponseHeaderNotPresent()
        {
            var response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
         
           var channelsController= new ChannelsController(_mockApiImplementation.Object,
                _mockDataParser.Object, _mockDataMapper.Object, new TimestampParser());

           context.Request.Headers["accept"] = "wewewe";
           channelsController.ControllerContext = new ControllerContext { HttpContext = context };
           _mockApiImplementation.Setup(x => x.GetCalculatedRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = channelsController.GetCalculatedChannels("100949474:239", "AirPressure", "DischargeRate", "2019-07-29T23:39:12.738Z", "2019-07-30T23:39:12.738Z", null!);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }


        #endregion
        [TestMethod]
        public void Test_GetChannelsByCode_ThrowsNotFoundExecption()
        {
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>()))
                                  .Throws(new NotFoundException("Error"));

            var result = _mockcontroller.GetChannelByCode("100949474:SPF74312A0123", "time", null!, null!);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));

        }

        [TestMethod]
        public void Test_GetChannelsByCode_ForCorrectResponse()
        {
            ApiResponse response = new ApiResponse(new SingleChannel());
            // Mock Http Headers
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockcontroller.ControllerContext = new ControllerContext { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockcontroller.GetChannelByCode("100949474:SPF74312A0123", "time", null!, null!);
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

        }

        [TestMethod]
        public void Test_PostChannels_WithOkResponse()
        {

            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(),
                                                         It.IsAny<DynamicInfluxRow[]>(),
                                                       It.IsAny<DataType>(), It.IsAny<string>()))
                                                .Returns(Task.FromResult<IActionResult>(new OkResult()));

            var result = _mockcontroller.PostChannels("100949474:SPF74312A0123", _dummyinputJson.ToObject<MultipleChannelsRequest>()!);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            _mockDataParser.Verify(o => o.ParseChannelsData(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsData(It.IsAny<JArray>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRows(It.IsAny<ParsedRows>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);

        }

        [TestMethod]
        public void Test_PostChannels_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(),
                                                           It.IsAny<DynamicInfluxRow[]>(),
                                                         It.IsAny<DataType>(), It.IsAny<string>()))
                                                  .Throws(new BadRequestException("Error"));
            var result = _mockcontroller.PostChannels("100949474:SPF74312A0123", _dummyinputJson.ToObject<MultipleChannelsRequest>() ?? throw new InvalidOperationException());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            _mockDataParser.Verify(o => o.ParseChannelsData(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsData(It.IsAny<JArray>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRows(It.IsAny<ParsedRows>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);
        }

        [TestMethod]
        public void Test_GetChannelDefinition_WithOkResponse()
        {
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(),
                                                                    It.IsAny<DataType>()))
                                  .Returns(Task.FromResult(new ChannelDefinitionClean[0]));
            var result = _mockcontroller.GetChannelDefinitions("100949474:SPF74312A0123");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Test_GetChannelDefinition_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(),
                                                                    It.IsAny<DataType>()))
                                   .Throws(new BadRequestException("Error"));
            var result = _mockcontroller.GetChannelDefinitions("100949474:SPF74312A0123");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public void Test_GetBulkChannels_WithOkResponse()
        {
            _mockApiImplementation.Setup(x => x.GetBulkRows(It.IsAny<WellKnownEntityId[]>(),
                                                          It.IsAny<TimePeriod>(), It.IsAny<string[]>(),
                                                          It.IsAny<DataType>()))
                                   .Returns(Task.FromResult(new BulkRows()));
            var result = _mockcontroller.GetBulkChannels("100949474:SPF74312A0123", null!, null!, "time");
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

        }

        [TestMethod]
        public void Test_GetBulkChannels_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.GetBulkRows(It.IsAny<WellKnownEntityId[]>(),
                                                         It.IsAny<TimePeriod>(), It.IsAny<string[]>(),
                                                         It.IsAny<DataType>()))
                                  .Throws(new BadRequestException("Error"));
            var result = _mockcontroller.GetBulkChannels("100949474:SPF74312A0123", null!, null!, "time");
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        }

        [TestMethod]
        public void Test_GetBulkChannels_WhenEquipmentWkeidIsNull()
        {
            var result = _mockcontroller.GetBulkChannels(null!, null!, null!, "time");
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }


        [TestMethod]
        public void Test_PostBulkChannels_WithOkResponse()
        {
            _mockApiImplementation.Setup(x => x.SaveRowsBulk(It.IsAny<(WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[]>(),
                                                             It.IsAny<DataType>(),
                                                             It.IsAny<string>()))
                                   .Returns(Task.FromResult(new BulkRows()));
            var result = _mockcontroller.PostBulkChannels(Get_PostBulkChannel_TestData("SendBulk.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            _mockDataParser.Verify(o => o.ParseChannels(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsBulk(It.IsAny<JToken>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRowsBulk(It.IsAny<ParsedRows[]>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);
        }

        [TestMethod]
        public void Test_PostBulkChannels_ThrowsException()
        {
            _mockApiImplementation.Setup(x => x.SaveRowsBulk(It.IsAny<(WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[]>(),
                                                             It.IsAny<DataType>(),
                                                             It.IsAny<string>()))
                                   .Throws(new BadRequestException("Error"));
            var result = _mockcontroller.PostBulkChannels(Get_PostBulkChannel_TestData("SendBulk.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

            _mockDataParser.Verify(o => o.ParseChannels(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsBulk(It.IsAny<JToken>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRowsBulk(It.IsAny<ParsedRows[]>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);
        }

        [TestMethod]
        public void Test_PostBulkChannels_WithMappings_WithOkResponse()
        {
            _mockApiImplementation.Setup(x => x.SaveRowsBulk(It.IsAny<(WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[]>(),
                                                             It.IsAny<DataType>(),
                                                             It.IsAny<string>()))
                                  .Returns(Task.FromResult(new BulkRows()));
            var result = _mockcontroller.PostBulkChannels(Get_PostBulkChannel_TestData("SendBulkWithMappings.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            _mockDataParser.Verify(o => o.ParseChannels(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsBulk(It.IsAny<JToken>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRowsBulk(It.IsAny<ParsedRows[]>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);
        }

        [TestMethod]
        public async Task Test_GetLatestTimeStampForChannels()
        {
            var mockTimestampChannel = new TimestampChannelData
            {
                Code = "AirPressure",
                LatestTimestamp = "2019-09-12T15:30:041Z",
                ThresholdTimestamp = "2019-07-29T15:30:04.431Z"
            };
            _mockApiImplementation.Setup(_ => _.GetChannelLatestTimeStampWithCode(It.IsAny<WellKnownEntityId>(), It.IsAny<string>(),
                It.IsAny<string>(),It.IsAny<string>())).Returns(Task.FromResult(new List<TimestampChannelData>()));
            var result = await 
                _mockcontroller.GetLatestTimeStampForChannels("100190051:SPF34305Y0074", mockTimestampChannel.Code,mockTimestampChannel.ThresholdTimestamp);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result,typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Test_GetLatestTimeStampForEmptyChannelCode()
        {
            var mockTimestampChannel = new TimestampChannelData
            {
                Code = null,
                LatestTimestamp = "2019-09-12T15:30:041Z",
                ThresholdTimestamp = "2019-07-29T15:30:04.431Z"
            };
            _mockApiImplementation.Setup(_ => _.GetChannelLatestTimeStampWithCode(It.IsAny<WellKnownEntityId>(), It.IsAny<string>(),
                It.IsAny<string>(),It.IsAny<string>())).Returns(Task.FromResult(new List<TimestampChannelData>()));
            var result = await
                _mockcontroller.GetLatestTimeStampForChannels("100190051:SPF34305Y0074", mockTimestampChannel.Code!,mockTimestampChannel.ThresholdTimestamp);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result,typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Test_GetLatestTimeStampForEmptyWkeid()
        {
            var mockTimestampChannel = new TimestampChannelData
            {
                Code = "COILCAT"
            };
            _mockApiImplementation.Setup(_ => _.GetChannelLatestTimeStampWithCode(It.IsAny<WellKnownEntityId>(), It.IsAny<string>(),
                It.IsAny<string>(),It.IsAny<string>())).Returns(Task.FromResult(new List<TimestampChannelData>()));
            var result = await
                _mockcontroller.GetLatestTimeStampForChannels("", mockTimestampChannel.Code, "2019-07-29T15:30:04.431Z");
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Test_GetLatestTimeStampForInvalidThresholdTimeStamp()
        {
            var mockTimeStampChannelData = new TimestampChannelData
            {
                Code = "SPF-743",
                ThresholdTimestamp = "2019-07-29",
                LatestTimestamp = "2019-07-29T15:30:04.431Z"
            };
            _mockApiImplementation.Setup(_ => _.GetChannelLatestTimeStampWithCode(It.IsAny<WellKnownEntityId>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(new List<TimestampChannelData>()));
            var result = await
                _mockcontroller.GetLatestTimeStampForChannels("100190051:SPF34305Y0074", mockTimeStampChannelData.Code, mockTimeStampChannelData.ThresholdTimestamp);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }


        private JToken Get_PostBulkChannel_TestData(string fileName)
        {
            return JToken.Parse(File.ReadAllText(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../EHC.API/JSON/ChannelData/Input", fileName))));
           
        }

        private JToken Get_Dummy_Json()
        {
            return JToken.Parse("\r\n{\r\n  \"meta\": {\r\n    \"channels\": [\r\n      {\r\n        \"code\": \"time\",\r\n        \"uom\": \"d\",\r\n        \"dimension\": \"time\"\r\n      },\r\n      {\r\n        \"code\": \"AirPressure\",\r\n        \"uom\": \"kPa\",\r\n        \"dimension\": \"pressure\"\r\n      },\r\n      {\r\n        \"code\": \"DischargePressure\",\r\n        \"uom\": \"kPa\",\r\n        \"dimension\": \"pressure\"\r\n      },\r\n      {\r\n        \"code\": \"DischargeRate\",\r\n        \"uom\": \"unitless\",\r\n        \"dimension\": \"ratio\"\r\n      }\r\n    ] \r\n  },\r\n  \"rows\": [\r\n    [\r\n      \"2019-07-29T15:30:02.156Z\",\r\n      655829.25,\r\n      null,\r\n      null\r\n    ],\r\n    [\r\n      \"2019-07-29T15:30:04.431Z\",\r\n      655830.84,\r\n      491541.375,\r\n      0.0870783925056458\r\n    ],\r\n    [\r\n      \"2019-07-29T15:30:07.293Z\",\r\n      null,\r\n      null,\r\n      0.0871633142232895\r\n    ]\r\n  ]\r\n}\r\n");
        }

        private ChannelsController GetChannelsController()
        {
            return new ChannelsController(_mockApiImplementation.Object,
                _mockDataParser.Object, _mockDataMapper.Object, _mockTimeStampParser.Object);
        }

    }
}
