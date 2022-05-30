using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NuGet.Frameworks;
using Polly.Caching;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class DataMapperTests
    {
        private static DataParser _dataParser;
        private static Mock<IChannelDefinitionService> _mockchannelDefinitionService;
        private static MockRepository _mockProvider;
        private static DataMapper _dataMapper;
        private static TimestampParser _mockTimeStampParser;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _dataParser = new DataParser();
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockTimeStampParser = new TimestampParser();
            _mockchannelDefinitionService = _mockProvider.Create<IChannelDefinitionService>();
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
        public void TestValidateAndMapChannels()
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

            var parsedChannels = _dataParser.ParseChannels(inputJson);
            _mockchannelDefinitionService.Setup(x => x.GetChannelDefinition("time", false)).ReturnsAsync(mockChannelDefinition[0]);
            _mockchannelDefinitionService.Setup(x => x.GetChannelDefinition("AirPressure", false)).ReturnsAsync(mockChannelDefinition[1]);
            _mockchannelDefinitionService.Setup(x => x.GetChannelDefinition("DischargePressure", false)).ReturnsAsync(mockChannelDefinition[2]);
            _mockchannelDefinitionService.Setup(x => x.GetChannelDefinition("DischargeRate", false)).ReturnsAsync(mockChannelDefinition[3]);
            DataMapper dataMapper = new DataMapper(_mockchannelDefinitionService.Object, _mockTimeStampParser);
            Task<ChannelDefinitionIndex[]> result = dataMapper.ValidateAndMapChannels(parsedChannels);
            Assert.IsTrue(result.Exception.Message.Contains("duplicate channel"));
        }
    }
}