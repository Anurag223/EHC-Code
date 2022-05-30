using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.HyperLinks;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.HyperLinks
{
    [UnitTestCategory]
    [TestClass]
    public class HyperLinksProviderTests
    {
        [TestMethod()]
        public void Test_Hyperlinks_ForChannel_SingleCode_PeriodSpecified()
        {
            // Arrange
            var provider = new HyperLinksProvider();

            var start = new DateTime(2019, 03, 21, 18, 47, 31, DateTimeKind.Utc);
            var end = new DateTime(2019, 03, 21, 20, 58, 19, DateTimeKind.Utc);

            var rowsRequest = new RowsRequest();

            rowsRequest.WKEid = WellKnownEntityId.Parse("H650932:414");
            rowsRequest.DataType = DataType.Channel;
            rowsRequest.QueryType = QueryType.SingleCode;
            rowsRequest.Codes = new string[]{ "AirPressure" };
            rowsRequest.TimePeriod = new TimePeriod(start, end);
            rowsRequest.ResponseFormat = ResponseFormat.Default;

            var timePeriod = new TimePeriod(start, end);

            // Act
            var hyperLinks = provider.GetHyperLinks(rowsRequest, timePeriod);

            // Assert
            hyperLinks.Should().HaveCount(3);
            hyperLinks.Should().ContainKeys("self", "previousPeriod", "equipment");

            hyperLinks["self"].Rel.Should().Be("self");
            hyperLinks["previousPeriod"].Rel.Should().Be("previousPeriod");
            hyperLinks["equipment"].Rel.Should().Be("equipment");

            hyperLinks["self"].Href.Should().Be(
            "/v2/equipment/H650932:414/channels/AirPressure?start=2019-03-21T18:47:31.000Z&end=2019-03-21T20:58:19.000Z");

            hyperLinks["previousPeriod"].Href.Should().Be(
            "/v2/equipment/H650932:414/channels/AirPressure?start=2019-03-20T18:47:31.000Z&end=2019-03-21T18:47:31.000Z");

            hyperLinks["equipment"].Href.Should().Be(
                "/v2/equipment/H650932:414");
        }


        [TestMethod()]
        public void Test_Hyperlinks_ForChannel_MultipleCodes_NoPeriodSpecified()
        {
            // Arrange
            var provider = new HyperLinksProvider();

            var rowsRequest = new RowsRequest();

            rowsRequest.WKEid = WellKnownEntityId.Parse("100196736:SBF62407Y0283");
            rowsRequest.DataType = DataType.Channel;
            rowsRequest.QueryType = QueryType.MultipleCodes;
            rowsRequest.Codes = new string[] { }; // all codes
            rowsRequest.TimePeriod = null;
            rowsRequest.ResponseFormat = ResponseFormat.Default;

            // found latest 24h period
            var start = new DateTime(2019, 07, 28, 03, 21, 08, DateTimeKind.Utc);
            var end = new DateTime(2019, 07, 29, 03, 21, 08, DateTimeKind.Utc);
            var timePeriod = new TimePeriod(start, end);

            // Act
            var hyperLinks = provider.GetHyperLinks(rowsRequest, timePeriod);

            // Assert
            hyperLinks["self"].Href.Should().Be(
                "/v2/equipment/100196736:SBF62407Y0283/channels");

            hyperLinks["previousPeriod"].Href.Should().Be(
                "/v2/equipment/100196736:SBF62407Y0283/channels?start=2019-07-27T03:21:08.000Z&end=2019-07-28T03:21:08.000Z");

            hyperLinks["equipment"].Href.Should().Be(
                "/v2/equipment/100196736:SBF62407Y0283");
        }

        [TestMethod()]
        public void Test_HyperlinksForReadingData_ForNoSpecificPeriod()
        {
            var hyperlinkBuilder = new HyperLinksProvider();
            var rowsRequest = new RowsRequest();

            rowsRequest.WKEid = WellKnownEntityId.Parse("100949474:1396539");
            rowsRequest.DataType = DataType.Reading;
            rowsRequest.QueryType = QueryType.MultipleCodes;
            rowsRequest.Codes = new string[] { }; // all codes
            rowsRequest.TimePeriod = null;
            rowsRequest.ResponseFormat = ResponseFormat.Default;

            // found latest 24h period
            var start = new DateTime(2019, 07, 28, 03, 21, 08, DateTimeKind.Utc);
            var end = new DateTime(2019, 07, 29, 03, 21, 08, DateTimeKind.Utc);
            var timePeriod = new TimePeriod(start, end);

            // Act
            var hyperLinks = hyperlinkBuilder.GetHyperLinks(rowsRequest, timePeriod);

            hyperLinks["self"].Href.Should().Be(
                "/v2/equipment/100949474:1396539/readings");

            hyperLinks["equipment"].Href.Should().Be(
                "/v2/equipment/100949474:1396539");
        }

        [TestMethod()]
        public void Test_HyperlinksEpisodicPointsData_For_SingleCode()
        {
            var provider = new HyperLinksProvider();

            var rowsRequest = new RowsRequest();

            rowsRequest.WKEid = WellKnownEntityId.Parse("100949474:1396539");
            rowsRequest.DataType = DataType.Episodic;
            rowsRequest.QueryType = QueryType.SingleCode;
            rowsRequest.Codes = new string[] { "AirPressure" };
            rowsRequest.TimePeriod = null;
            rowsRequest.ResponseFormat = ResponseFormat.Default;

            // found latest 24h period
            var start = new DateTime(2019, 07, 28, 03, 21, 08, DateTimeKind.Utc);
            var end = new DateTime(2019, 07, 29, 03, 21, 08, DateTimeKind.Utc);
            var timePeriod = new TimePeriod(start, end);

            // Act
            var hyperLinks = provider.GetHyperLinks(rowsRequest, timePeriod);

            hyperLinks["self"].Href.Should().Be(
                "/v2/equipment/100949474:1396539/episodic-points/AirPressure");

            hyperLinks["previousPeriod"].Href.Should().Be(
                "/v2/equipment/100949474:1396539/episodic-points/AirPressure?start=2019-07-27T03:21:08.000Z&end=2019-07-28T03:21:08.000Z");

            hyperLinks["equipment"].Href.Should().Be(
                "/v2/equipment/100949474:1396539");
        }

        [TestMethod()]
        public void Test_HyperlinksEpisodeData_ForValidEpisodeId()
        {
            var provider = new HyperLinksProvider();

            var rowsRequest = new RowsRequest();

            rowsRequest.EpisodeId = "3bf3342349dnc";
            rowsRequest.WKEid = WellKnownEntityId.Parse("100949474:1396539");
            rowsRequest.DataType = DataType.Episodic;
            rowsRequest.QueryType = QueryType.SingleCode;
            rowsRequest.Codes = new string[] { "AirPressure" };
            rowsRequest.TimePeriod = null;
            rowsRequest.ResponseFormat = ResponseFormat.Default;

            // found latest 24h period
            var start = new DateTime(2019, 07, 28, 03, 21, 08, DateTimeKind.Utc);
            var end = new DateTime(2019, 07, 29, 03, 21, 08, DateTimeKind.Utc);
            var timePeriod = new TimePeriod(start, end);

            // Act
            var hyperLinks = provider.GetHyperLinks(rowsRequest, timePeriod);

            hyperLinks["self"].Href.Should().Be(
                "/v2/equipment/100949474:1396539/episodic-points/AirPressure?episodeId=3bf3342349dnc");

            hyperLinks["previousPeriod"].Href.Should().Be(
                "/v2/equipment/100949474:1396539/episodic-points/AirPressure?episodeId=3bf3342349dnc&start=2019-07-27T03:21:08.000Z&end=2019-07-28T03:21:08.000Z");

            hyperLinks["equipment"].Href.Should().Be(
                "/v2/equipment/100949474:1396539");
        }
    }
}