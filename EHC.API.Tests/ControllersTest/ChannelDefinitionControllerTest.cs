using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Querying;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class ChannelDefinitionControllerTest
    {
        private MockRepository _mockProvider;
        private Mock<IChannelDefinitionService> _mockChannelDefinitionService;
        private Mock<IGetCollectionFromCacheStrategy<ChannelDefinition>> _mockmultiResourceGetter;
        private Mock<IGetFromCacheStrategy<ChannelDefinition>> _mockSingleResourceGetter;
        private DataParser _dataParser;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockChannelDefinitionService = _mockProvider.Create<IChannelDefinitionService>();
            _mockmultiResourceGetter = _mockProvider.Create<IGetCollectionFromCacheStrategy<ChannelDefinition>>();
            _mockSingleResourceGetter = _mockProvider.Create<IGetFromCacheStrategy<ChannelDefinition>>();
            _dataParser = new DataParser();
        }

        [TestMethod]
        public void Class_Should_Be_Decorated()
        {
            ApiBaseTest.ValidateClassAttributes<ChannelDefinitionsController>();
        }

        [TestMethod]
        public void Get_Should_Be_Decorated()
        {
            ApiBaseTest.ValidateGetMethodsWithBadRequestAttributes<ChannelDefinitionsController>();
            ApiBaseTest.ValidateGetMethodsWithNotAcceptableAttributes<ChannelDefinitionsController>();
            ApiBaseTest.ValidateGetMethodsWithProduceOkAttributes<ChannelDefinition,ChannelDefinitionsController>();
        }

        protected ChannelDefinitionsController GetChannelDefinitionsController()
        {
            return new ChannelDefinitionsController(_mockChannelDefinitionService.Object,
                _mockmultiResourceGetter.Object);
        }

        private List<ChannelDefinition> mockChannelDefinition = new List<ChannelDefinition>()
        {
            new ChannelDefinition() {
                Code = "time",
                Name = "time",
                Dimension = "time",
                Uom = "d",
                Type = "Channel",
                LegalClassification = "EHC"
            },
            new ChannelDefinition(){
                Code = "AirPressure",
                Name = "AirPressure",
                Dimension = "pressure",
                Uom = "kPa",
                Type = "Channel",
                LegalClassification = "EHC"
            },
            new ChannelDefinition(){
                Code = "DischargePressure",
                Name = "DischargePressure",
                Dimension = "pressure",
                Uom = "kPa",
                Type = "Channel",
                LegalClassification = "EHC"
            },
            new ChannelDefinition(){
                Code = "DischargeRate",
                Name = "DischargeRate",
                Dimension = "ratio",
                Uom = "unitless",
                Type = "Channel",
                LegalClassification = "EHC"
            },

        };

        [TestMethod]
        public async Task Test_GetChannelDefinition()
        {
            var channelDefinitionController = GetChannelDefinitionsController();

            var querySpec = QuerySpec.ForEverything;

            IActionResult result = new OkResult();
            _mockmultiResourceGetter.Setup(x => x.GetCollection(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(result));

            var actualResult = await channelDefinitionController.GetChannelDefinitions(querySpec);

            Assert.IsInstanceOfType(actualResult, typeof(OkResult));
        }

        [TestMethod]
        public async Task Test_GetChannelDefinitionByCode()
        {
            var channelObject = new ChannelDefinition
            {
                Code = "time",
                Name = "time",
                Dimension = "time",
                Uom = "d",
                Type = "Channel",
                LegalClassification = "EHC"
            };

            var objectResult = new OkObjectResult(channelObject);
            _mockSingleResourceGetter.Setup(x =>
                x.GetSingleRepresentationById<ChannelDefinitionsController>(It.Is<string>(s => s == channelObject.Code),
                    It.IsAny<QuerySpec>())).Returns(Task.FromResult<IActionResult>(objectResult));

            var channelDefinitionController = GetChannelDefinitionsController();

            _mockChannelDefinitionService.Setup(x => x.GetChannelDefinition(channelObject.Code, false))
                .ReturnsAsync(mockChannelDefinition[0]);

            var resultData = await channelDefinitionController.GetChannelDefinitionByCode(channelObject.Code);
            resultData.Should().NotBeNull();
            ((OkObjectResult)resultData).Value.Should().BeOfType<ChannelDefinition>(channelObject.Code);
            Assert.IsInstanceOfType(resultData, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Test_CreateChannelDefinition()
        {
            var inputJson = JToken.Parse(@"
{
  ""meta"": {
    ""channels"": [
      {
        ""index"":0,
        ""code"": ""time"",
        ""uom"": ""d"",
        ""dimension"": ""time""
      },
      {
        ""index"":1,
        ""code"": ""AirPressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
       ""index"":2,
        ""code"": ""DischargePressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
        ""index"":3,
        ""code"": ""DischargeRate"",
        ""uom"": ""unitless"",
        ""dimension"": ""ratio""
      },
       {
        ""index"":4,
        ""code"": ""DischargeRate"",
        ""uom"": ""unitless"",
        ""dimension"": ""ratio""
      }
      
    ] 
  },
  ""rows"": [
    [
      ""2019-07-29T15:30:02.156Z"",
      655829.25,
      null,
      null
    ],
    [
      ""2019-07-29T15:30:04.431Z"",
      655830.84,
      491541.375,
      0.0870783925056458,
      10.0871633142232895
    ],
    [
      ""2019-07-29T15:30:07.293Z"",
      null,
      null,
      0.0871633142232895
    ]
  ]
}
");

            _dataParser.ParseChannels(inputJson);
            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(x => x.GetChannelDefinition("time", false))
                .ReturnsAsync(mockChannelDefinition[0]);
            _mockChannelDefinitionService.Setup(x => x.CreateChannelDefinition(It.IsAny<ChannelDefinition>())).ReturnsAsync(mockChannelDefinition[0]);

            var result = await channelDefinitionController.CreateChannelDefinition(inputJson.ToObject<ChannelDefinition>());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        [TestMethod]
        public async Task Test_DeleteChannelDefinition()
        {
            var channelDefinitionController = GetChannelDefinitionsController();
            var deleteChannel = await channelDefinitionController.DeleteChannelDefinition("time");
            deleteChannel.Should().NotBeNull();
            Assert.IsInstanceOfType(deleteChannel, typeof(OkResult));
        }

        [TestMethod]
        public async Task Test_UpdateChannelDefinition()
        {
            var inputJson = JToken.Parse(@"
               {
                ""code"":""time"",
                ""name"":""time"",
                ""dimension"":""time"",
                ""uom"":""d"",
                ""Type"":""Channel"",
                ""LegalClassification"":""EHC""}
            ");

            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(x => x.UpdateChannelDefinition(It.IsAny<ChannelDefinition>()))
                .Returns(Task.FromResult(It.IsAny<IActionResult>()));

            var updateChannel = await channelDefinitionController.UpdateChannelDefinition("time", inputJson.ToObject<ChannelDefinition>());
            updateChannel.Should().NotBeNull();
            Assert.IsInstanceOfType(updateChannel, typeof(NoContentResult));
        }

        
        [TestMethod]
        public async Task Test_UpdateChannelDefinition_ForException()
        {
            var inputJson = JToken.Parse(@"
               {
                ""code"":""AirPressure"",
                ""name"":""AirPressure"",
                ""dimension"":""Pressure"",
                ""uom"":""kPa"",
                ""Type"":""Channel"",
                ""LegalClassification"":""EHC""}
            ");

            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(x => x.UpdateChannelDefinition(It.IsAny<ChannelDefinition>()))
                .Returns(Task.FromResult(It.IsAny<IActionResult>()));

            var updateChannel = await channelDefinitionController.UpdateChannelDefinition("time", inputJson.ToObject<ChannelDefinition>());
            updateChannel.Should().NotBeNull();
            Assert.IsInstanceOfType(updateChannel, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Test_UpdateChannelDefinition_ForNotFoundException_WhenEquipmentCodeNotFound()
        {
            string invalidEquipmentCode = "SPF-333";
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom",
                EquipmentCodes = new List<string>() { invalidEquipmentCode }
            };

            var channelDefinitionController = new ChannelDefinitionsController(_mockChannelDefinitionService.Object,
                _mockmultiResourceGetter.Object);

            _mockChannelDefinitionService.Setup(x => x.UpdateChannelDefinition(It.IsAny<ChannelDefinition>())).Throws(
                new NotFoundException($"Equipment code {invalidEquipmentCode.Substring(2)} not found in Epic V3 Hierarchy")
                    { ErrorCode = ErrorCodes.EquipmentCodeNotFound });

            var updatedChannelDefinition = await channelDefinitionController.UpdateChannelDefinition(cd.Code,cd);
            Assert.IsInstanceOfType(updatedChannelDefinition, typeof(NotFoundObjectResult));
        }


        [TestMethod]
        public async Task Test_DeleteChannelDefinition_ForException()
        {
            var channelDefinition = new ChannelDefinition
            {
                Code = "",
                Name = "AirPressure",
            };
            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(x => x.DeleteChannelDefinition("")).Throws(new NotFoundException("Code not found"));
            var deleteChannel = await channelDefinitionController.DeleteChannelDefinition(channelDefinition.Code);
            deleteChannel.Should().NotBeNull();
            Assert.IsInstanceOfType(deleteChannel, typeof(NotFoundObjectResult));
        }

        [TestMethod]
        public async Task Test_CreateChannelDefinition_ForException()
        {
            var inputJson = JToken.Parse(@"
{
  ""meta"": {
    ""channels"": [
      {
        ""index"":0,
        ""code"": ""time"",
        ""uom"": ""d"",
        ""dimension"": ""time""
      },
      {
        ""index"":1,
        ""code"": ""AirPressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
       ""index"":2,
        ""code"": ""AirPressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
    ] 
  },
  ""rows"": [
    [
      ""2019-07-29T15:30:02.156Z"",
      655829.25,
      null,
      null
    ],
    [
      ""2019-07-29T15:30:04.431Z"",
      655830.84,
      491541.375,
      0.0870783925056458,
      10.0871633142232895
    ],
    [
      ""2019-07-29T15:30:07.293Z"",
      null,
      null,
      0.0871633142232895
    ]
  ]
}
");
            _dataParser.ParseChannels(inputJson);
            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(x => x.GetChannelDefinition("AirPressure", false))
                .ReturnsAsync(mockChannelDefinition[1]);
            _mockChannelDefinitionService.Setup(x => x.CreateChannelDefinition(It.IsAny<ChannelDefinition>())).Throws(new BadRequestException("Channel Definition already exists"));
            var result = await channelDefinitionController.CreateChannelDefinition(inputJson.ToObject<ChannelDefinition>());
            result.Should().NotBeNull();
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Test_CreateChannelDefinition_ForNotFoundException_WhenEquipmentCodeNotFound()
        {
            string invalidEquipmentCode = "SPF-333";
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom",
                EquipmentCodes = new List<string>() { invalidEquipmentCode }
            };

            var channelDefinitionController = new ChannelDefinitionsController(_mockChannelDefinitionService.Object,
                _mockmultiResourceGetter.Object);

            _mockChannelDefinitionService.Setup(x => x.CreateChannelDefinition(It.IsAny<ChannelDefinition>())).Throws(
                new NotFoundException($"Equipment code {invalidEquipmentCode.Substring(2)} not found in Epic V3 Hierarchy")
                    { ErrorCode = ErrorCodes.EquipmentCodeNotFound });

            var createdChannelDefinition = await channelDefinitionController.CreateChannelDefinition(cd);
            Assert.IsInstanceOfType(createdChannelDefinition, typeof(NotFoundObjectResult));
        }

        #region UpdateChannelDefinitionWithEqCodes

        [TestMethod]
        public async Task Test_UpdateChannelDefinitionWithEqCodes_Successful()
        {
            List<string> inputChannelCodes = new List<string>() { "AirPressure", "DischargeRate" };

            var channelDefinitionController = GetChannelDefinitionsController();
            _mockChannelDefinitionService.Setup(o => o.ValidateChannelCode(inputChannelCodes)).Returns(Task.FromResult(true));
            _mockChannelDefinitionService.Setup(o => o.ValidateEquipmentCode("SPF-743")).Returns(Task.FromResult(true));
            _mockChannelDefinitionService.Setup(o => o.UpdateEquipmentCodeOnChannelDefinition("SPF-743", inputChannelCodes)).Returns(Task.FromResult(true));

            var updateChannelDefinition = await channelDefinitionController.UpdateChannelDefinitionWithEqCodes("SPF-743", inputChannelCodes);
            Assert.IsInstanceOfType(updateChannelDefinition, typeof(OkResult));
        }
       
        [TestMethod]
        public async Task Test_UpdateChannelDefinitionWithEqCodes_ThrowsBadRequestException_WhenBodyIsNull()
        {
            var channelDefinitionController = GetChannelDefinitionsController();

            var updateChannelDefinition = await channelDefinitionController.UpdateChannelDefinitionWithEqCodes("SPF-743", null);
            Assert.IsInstanceOfType(updateChannelDefinition, typeof(BadRequestObjectResult));
        }

        [TestMethod]
        public async Task Test_UpdateChannelDefinitionWithEqCodes_ThrowsNotFoundException_ValidationFails()
        {
            List<string> inputChannelCodes = new List<string>() { "AirPressure", "DischargeRate" };
            string code = "SPF-333";

            var channelDefinitionController= new ChannelDefinitionsController(_mockChannelDefinitionService.Object,
                _mockmultiResourceGetter.Object);

            _mockChannelDefinitionService.Setup(x=> x.ValidateEquipmentCode(It.IsAny<string>())).Throws(
                new NotFoundException($"Equipment code {code.Substring(2)} not found in Epic V3 Hierarchy")
                { ErrorCode = ErrorCodes.EquipmentCodeNotFound });

            var updateChannelDefinition = await channelDefinitionController.UpdateChannelDefinitionWithEqCodes(code, inputChannelCodes);
            Assert.IsInstanceOfType(updateChannelDefinition, typeof(NotFoundObjectResult));
        }
        #endregion
    }







}
