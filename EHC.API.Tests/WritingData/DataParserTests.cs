using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Common;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;

namespace EHC.API.Tests.WritingData
{
    [UnitTestCategory]
    [TestClass]
    public class DataParserTests
    {
        private static DataParser _dataParser;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _dataParser = new DataParser();
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            _dataParser = null;
        }



        [TestMethod]
        public void BadRequestException_No_Properties()
        {
            var inputJson = JToken.Parse(@"{ ""name"": ""John"" }");

            _dataParser.Invoking(x => x.ParseRows(inputJson))
                .Should().Throw<BadRequestException>();
        }


        [TestMethod]
        public void BadRequestException_Property_Channels()
        {
            var inputJson = JToken.Parse(@"
{ 
    ""meta"": {
        ""some_channels"": [1, 2, 3] 
    }, 
    ""rows"": [1, 2, 3]
}"
            );

            _dataParser.Invoking(x => x.ParseChannels(inputJson))
                .Should().Throw<BadRequestException>();
        }



        [TestMethod]
        public void BadRequestException_Property_Rows()
        {
            var inputJson = JToken.Parse(@"
{ 
    ""meta"": {
        ""channels"": [1, 2, 3] 
    }, 
    ""rows"": 101
}"
            );

            _dataParser.Invoking(x => x.ParseRows(inputJson))
                .Should().Throw<BadRequestException>();
        }


        [TestMethod]
        public void ParseImplicit()
        {
            var inputJson = JToken.Parse(@"
{
  ""meta"": {
    ""channels"": [
      {
        ""code"": ""time"",
        ""uom"": ""d"",
        ""dimension"": ""time""
      },
      {
        ""code"": ""AirPressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
        ""code"": ""DischargePressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
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
      0.0870783925056458
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
            var parsedRows = _dataParser.ParseRows(inputJson);
            CheckParsedData(parsedRows, parsedChannels, true);
        }



        [TestMethod]
        public void ParseExplicit()
        {
            var inputJson = JToken.Parse(@"
{
  ""meta"": {
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
        ""code"": ""DischargePressure"",
        ""uom"": ""kPa"",
        ""dimension"": ""pressure""
      },
      {
        ""index"": 3,
        ""code"": ""DischargeRate"",
        ""uom"": ""unitless"",
        ""dimension"": ""ratio""
      }
    ]
  },
  ""rows"": [
    [
        [0, ""2019-07-29T15:30:02.156Z""],
        [1, 655829.25]
    ],[
        [0, ""2019-07-29T15:30:04.431Z""],
        [1, 655830.84],
        [2, 491541.375],
        [3, 0.0870783925056458]
    ],[
        [0, ""2019-07-29T15:30:07.293Z""],
        [3, 0.0871633142232895]
    ]
  ]
}
");

            var parsedChannels = _dataParser.ParseChannels(inputJson);
            var parsedRows = _dataParser.ParseRows(inputJson);
            CheckParsedData(parsedRows, parsedChannels, true);
        }


        private void CheckParsedData(ParsedRows parsedRows, ParsedChannels parsedChannels, bool implict)
        {
            parsedChannels.Channels.Should().HaveCount(4);
            var lastChannel = parsedChannels.Channels[3];

            lastChannel.Code.Should().Be("DischargeRate");
            lastChannel.Uom.Should().Be("unitless");
            lastChannel.Dimension.Should().Be("ratio");

            parsedRows.Rows.Should().HaveCount(3);

            var lastRow = parsedRows.Rows[2];
            lastRow.Values[0].Value.Should().Be(DateTime.Parse("2019-07-29T15:30:07.293Z").ToUniversalTime());

            if (implict)
            {
                // lastRow.Values["AirPressure"].Should().BeNull();
                // lastRow.Fields["DischargePressure"].Should().BeNull();
            }
            
            // lastRow.Fields["DischargeRate"].Should().Be(0.0871633142232895D);

            // OUTDATED !
            // classes have been changed in April 2020
        }



    }
}


