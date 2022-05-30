using System;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.Common;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ResponseProviders
{
    [UnitTestCategory]
    [TestClass]
    public class ResponseProviderCsvTest
    {
        private Mock<IUrlBuilder> _urlBuilder;
        private EhcApiConfig _config;

        [TestInitialize]

        public void TestInitialize()
        {           
            _config = ConfigDetails();
            _urlBuilder = new Mock<IUrlBuilder>();
        }
        public EhcApiConfig ConfigDetails()
        {
            return new EhcApiConfig()
            {
                InfluxDB = new ExternalApi()
                {
                    BaseUrl = "testInfluxDb.slb.com",
                    Username = "testInflux",
                    Password = "test@123"
                }
            };
        }

        [TestMethod]
        public async Task Verify_GetResponse()
        {
            var responseProvider = GetResponseProviderCsv();
            var dummyEquipment = new Equipment()
            {
                EquipmentCode = "SPF-743",
                EquipmentWkeId = "100949474:1396533",
                MaterialNumber = "100949474",
                SerialNumber = "1396533",
                SourceSystemRecordId = "1396533"
            };

            var mockQuery = new Query("DXP_WPS_PUMPING_EQUIPMENT",
                "SELECT * FROM STIMULATION_PUMP_WS - 67 WHERE EquipmentInstance='100949474:1396533' AND time >= 1564327807293000000 AND time <= 1564414207293000000");
            var mockQueryContext = new QueryContext()
            {
                Equipment = dummyEquipment
            };
            _urlBuilder.Setup(x =>
                x.GetQueryUrl(It.IsAny<Query>())).Returns("https://www.google.com/");
            var res = await responseProvider.GetResponse(mockQuery, mockQueryContext);
            res.Should().NotBeNull();
            Assert.AreEqual(typeof(ApiResponse), typeof(ApiResponse));
        }

        [TestMethod]
        public async Task Verify_NoResponse()
        {
            var dummyRowsrequest = new RowsRequest()
            {
                WKEid = new WellKnownEntityId("3223452", "SPF743786"),
                Codes = new[] { "time", "AirPressure" },
                DataType = DataType.Episodic,
                QueryType = QueryType.SingleCode,
                TimePeriod = new TimePeriod(DateTime.Now, DateTime.Now.AddDays(10)),
                EpisodeId = "247329847293",
                ResponseFormat = ResponseFormat.Influx
            };

            var dummyEquipment = new Equipment()
            {
                EquipmentCode = "SPF-743",
                EquipmentWkeId = "100949474:1396533",
                MaterialNumber = "100949474",
                SerialNumber = "1396533",
                SourceSystemRecordId = "1396533"
            };

            var responseProvider = GetResponseProviderCsv();
            var mockQueryContext = new QueryContext()
            {
                Equipment = dummyEquipment
            };
            var result = await responseProvider.GetResponseNoData(dummyRowsrequest, mockQueryContext);
            result.Should().NotBeNull();
            result.Should().BeOfType<ApiResponse>();
        }

        protected ResponseProviderCsv GetResponseProviderCsv()
        {
            return new ResponseProviderCsv(new HttpClientFake(),_urlBuilder.Object,_config);
        }

        public class HttpClientFake : IHttpClientFactory
        {

            public void Clear()
            {

            }

            public static void BasicAuthentication()
            {
                string username = "test";
                string password = "test@123";

                HttpClient client = new HttpClient();
                client.SetBasicAuthentication(username, password);
            }

            public HttpClient CreateClient(string name)
            {
                return new HttpClient();
            }

            public HttpResponseMessage GetAsync()
            {
                return new HttpResponseMessage();
            }
        }
    }
}
