using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.API.Services;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Querying;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.Common;
using TLM.EHC.API.WritingData;
using FizzWare.NBuilder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;
using TLM.EHC.Common.Historian;
using System.IO;
using Microsoft.Azure.KeyVault.Models;
using NSubstitute;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;
using TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class EpisodicPointsControllerTest
    {
        private MockRepository _mockProvider;
        private Mock<IApiImplementation> _mockApiImplementation;
        private Mock<IDataParser> _mockDataParser;
        private Mock<IDataMapper> _mockDataMapper;
        private Mock<IEpisodeService> _mockEpisodeService;
        private Mock<ITimestampParser> _mockTimeStampParser;
        private EpisodicPointsController _mockEpisodicPointsController;
        private JToken _dummyInputJson;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockApiImplementation = _mockProvider.Create<IApiImplementation>();
            _mockEpisodeService = _mockProvider.Create<IEpisodeService>();
            _mockDataParser = _mockProvider.Create<IDataParser>();
            _mockDataMapper = _mockProvider.Create<IDataMapper>();
            _mockTimeStampParser = _mockProvider.Create<ITimestampParser>();
            _mockEpisodicPointsController = GetEpisodicPointsController();
            _dummyInputJson = Get_Dummy_Json();
        }

        [TestCleanup]
        public void CleanUp()
        {
            _mockProvider = null;
            _mockEpisodeService = null;
            _mockApiImplementation = null;
            _mockDataParser = null;
            _mockDataMapper = null;
            _mockTimeStampParser = null;
            _mockEpisodicPointsController = null;
            _dummyInputJson = null;
        }


        [TestMethod]
        public void Get_Should_Be_Decorated()
        {
            ApiBaseTest.ValidateGetMethodsWithBadRequestAttributes<EpisodicPointsController>();
            ApiBaseTest.ValidateGetMethodsWithProduceOkAttributes<MultipleChannels, EpisodicPointsController>();
        }

        [TestMethod]
        public void Test_Get_EpisodicPoints_Returns_Ok()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockEpisodicPointsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));

            var result = _mockEpisodicPointsController.GetEpisodicPoints("100949474:SPF74312A0123", "5f6287c08859500001e3f98a", null, null, "time,code");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));

        }

        [TestMethod]
        public void Test_Get_EpisodicPoints_ThrowsNotFoundException()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockEpisodicPointsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>()))
                                  .Throws(new NotFoundException("Error"));

            var result = _mockEpisodicPointsController.GetEpisodicPoints("100949474:SPF74312A0123", "5f6287c08859500001e3f98a", null, null, "time,code");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));

        }

        [TestMethod]
        public void Test_Get_EpisodicPoints_ByCode_ThrowsNotFoundException()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockEpisodicPointsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>()))
                .Throws(new NotFoundException("Error"));
            var result = _mockEpisodicPointsController.GetEpisodicPointsByCode("100949474:SPF74312A0123", "5f6287c08859500001e3f98a", null, null, "time,code");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void Test_Get_EpisodicPoints_ByCode_Returns_Ok()
        {
            ApiResponse response = new ApiResponse(new MultipleChannels());
            var context = new DefaultHttpContext();
            context.Request.Headers["accept"] = "application/json";
            _mockEpisodicPointsController.ControllerContext = new ControllerContext() { HttpContext = context };
            _mockApiImplementation.Setup(x => x.GetRows(It.IsAny<RowsRequest>())).Returns(Task.FromResult(response));
            var result = _mockEpisodicPointsController.GetEpisodicPointsByCode("100949474:SPF74312A0123", "5f6287c08859500001e3f98a", null, null, "time,code");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Test_Get_EpisodicPointDefinitions_ByCode_Returns_Ok()
        {
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(), DataType.Episodic)).Returns(Task.FromResult(new ChannelDefinitionClean[0]));
            var result = _mockEpisodicPointsController.GetEpisodicPointsDefinitions("100949474:SPF74312A0123");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        }

        [TestMethod]
        public void Test_Get_EpisodicPointDefinitions_ByCode_ThrowsNotFoundException()
        {
            _mockApiImplementation.Setup(x => x.GetChannelDefinitions(It.IsAny<WellKnownEntityId>(), DataType.Episodic)).Throws(new NotFoundException("Error"));
            var result = _mockEpisodicPointsController.GetEpisodicPointsDefinitions("100949474:SPF74312A0123");
            result.Result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public void Test_Post_EpisodicPoints_Returns_Ok()
        {
            var dummyEpisode = new Episode()
            {
                Id = "5f6287c08859500001e3f98a",
                Name = "test",
                Tags = new List<string>(),
                EquipmentWkeIdList = new List<string>() { "100949474:SPF74312A0123" },
                Relationships = new Dictionary<string, EpisodeRelationship>()
            };
            _mockEpisodeService.Setup(s => s.GetEpisodeById("5f6287c08859500001e3f98a"))
                .Returns(Task.FromResult(dummyEpisode));
            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(),
                    It.IsAny<DynamicInfluxRow[]>(),
                    It.IsAny<DataType>(), "5f6287c08859500001e3f98a"))
                .Returns(Task.FromResult<IActionResult>(new OkResult()));
            var result = _mockEpisodicPointsController.PostEpisodicPoints("100949474:SPF74312A0123", _dummyInputJson.ToObject<EpisodicPointsRequest>());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            _mockDataParser.Verify(o => o.ParseChannelsData(It.IsAny<JToken>()), Times.Once);
            _mockDataParser.Verify(o => o.ParseRowsData(It.IsAny<JArray>()), Times.Once);
            _mockDataMapper.Verify(o => o.ValidateAndMapChannels(It.IsAny<ParsedChannels>()), Times.Once);
            _mockDataMapper.Verify(o => o.MapToInfluxRows(It.IsAny<ParsedRows>(), It.IsAny<ChannelDefinitionIndex[]>()), Times.Once);
        }

        [TestMethod]
        public void Test_Post_EpisodicPoints_ThrowsBadRequestException()
        {
            EpisodicPointsRequest varMultipleChannels = _dummyInputJson.ToObject<EpisodicPointsRequest>();
            varMultipleChannels.Meta.EpisodeId = null;
            _mockApiImplementation.Setup(x => x.SaveRows(It.IsAny<WellKnownEntityId>(),
                    It.IsAny<DynamicInfluxRow[]>(),
                    It.IsAny<DataType>(), "5f6287c08859500001e3f98a"))
                .Throws(new BadRequestException("Error"));
            var result = _mockEpisodicPointsController.PostEpisodicPoints("100949474:SPF74312A0123", varMultipleChannels);
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        }

        private JToken Get_Dummy_Json()
        {
            return JToken.Parse(@"
            {
                ""meta"": {
                    ""episodeId"": ""5ff865b5191963b4958b7f3d"",
                    ""channels"": [
                    {
                        ""index"":  0,
                        ""code"": ""time"",
                        ""uom"": ""d"",
                        ""dimension"": ""time""
                    },
                    {
                        ""index"":  1,
                        ""code"": ""AirPressure"",
                        ""uom"": ""kPa"",
                        ""dimension"": ""pressure""
                    },
                    {
                        ""index"": 2,
                        ""code"": ""CMT_TRUCK_FLUID_VOL_TOT"",
                        ""uom"": ""m3"",
                        ""dimension"": ""volume""
                    },
                    {
                        ""index"": 3,
                        ""code"": ""DischargeRate"",
                        ""uom"": ""m3 / sec"",
                        ""dimension"": ""Flowrate""
                    }
                    ]
                },
                ""rows"": [
                    [
                    [0, ""2019 - 07 - 29T15: 30:02.156Z""],
                    [1, 655829.25]
                    ],[
                    [0, ""2019 - 07 - 29T15: 30:04.431Z""],
                    [1, 655830.84],
                    [2, 491541.375],
                    [3, 0.0870783925056458]
                    ],[
                    [0, ""2019 - 07 - 29T15: 30:07.293Z""],
                    [3, 0.0871633142232895]
                    ]
                    ]
            }");
        }

        private EpisodicPointsController GetEpisodicPointsController()
        {
            return new EpisodicPointsController(_mockTimeStampParser.Object, _mockDataParser.Object, _mockDataMapper.Object,
                _mockEpisodeService.Object, _mockApiImplementation.Object);
        }

    }
}
