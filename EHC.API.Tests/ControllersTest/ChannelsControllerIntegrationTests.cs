using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.API.Services;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.WritingData;
using System.Threading.Tasks;
using TLM.EHC.Common.Historian;
using System.IO;
using System.Linq;
using System.Net.Http;
using Autofac;
using EHC.API.Tests.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.Admin;
using TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.API.HyperLinks;
using TLM.EHC.API.ResponseProviders;
using Tlm.Sdk.Core.Data;
using System.Linq.Expressions;
using System.Net;
using Autofac.Core;
using EHC.API.Tests.Mocks;
using Microsoft.AspNetCore.Http;
using Vibrant.InfluxDB.Client.Rows;
using TLM.EHC.Common.Services;
using TLM.EHC.Common.Clients.EquipmentApi;
using TLM.EHC.Common.Clients.EquipmentModelApi;
using TLM.EHC.Common.Clients.EpicV3Api;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using AttributeValue = TLM.EHC.Common.Clients.EpicV3Api.AttributeValue;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class ChannelsControllerIntegrationTests
    {
        private MockRepository _mockProvider;
        private IApiImplementation _apiImplementation;
        private IDataParser _dataParser;
        private IDataMapper _dataMapper;
        private ITimestampParser _timeStampParser;
        private IChannelDefinitionService _channelDefinitionService;
        private Mock<IRepositoryHandler<ChannelDefinition>> _mockChannelDefinitionRepositoryHandler;
        private ChannelsController _sut;

        //EHC API dependencies
        private IEquipmentProvider _equipmentProvider;
        private Mock<IEquipmentApiClient> _mockEquipmentApiClient;
        private IMemoryCache _memoryCache;
        private Mock<IEquipmentModelApiClient> _mockEquipmentModelClient;
        private IEpicV3HierarchyProvider _epicV3HierarchyProvider;
        private Mock<IEpicV3ApiClient> _mockEpicV3ApiClient;
        private IResponseProviderResolver _responseProviderResolver;
        private IContainer _autofacContainer;
        private ContainerBuilder _autofacBuilder;
        private IHistorianClient _historianClient;
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private IUrlBuilder _urlBuilder;
        private EhcApiConfig _ehcApiConfig;
        private IHyperLinksProvider _hyperLinksProvider;
        private Mock<IHistorianWriter> _historianWriter;
        private IEpisodeService _episodeService;
        private Mock<IRepositoryHandler<Episode>> _mockEpisodeRepositoryHandler;
        private IInfluxDBMappingService _influxDbMappingService;
        private Mock<IRepositoryHandler<InfluxDBMapping>> _mockInfluxDbMappingRepositoryHandler;
        private Mock<IUrlBuilder> _mockUrlBuilder;
        private HttpClient _client;

        private InfluxDBMapping[] _defaultMappings = new InfluxDBMapping[5];

        #region TestDataStringConstants

        private const string Uc1DeviceId1 = "H730200:091";
        private const string Uc1DeviceId2 = "H730200:116";
        private const string Uc1DeviceId3 = "H730200:233";

        private const string MeasurementName = "MeasurementName";

        private const string SiteId1 = "PC-RCAVE-02:6ID1";
        private const string SiteId2 = "PC-RCAVE-02:6ID2";
        private const string SiteId3 = "PC-RCAVE-02:6ID3";
        private const string SiteId4 = "PC-RCAVE-02:6B24";

        private const string FieldNameSiteId = "SiteID";
        private const string FieldNameDischargeRate = "DischargePressure.DischargesPerMinute";
        private const string BarrelsPerMinRate = "Rate.BarrelsPerMin";

        private const string DbName1 = "DBNAME1";
        private const string DbName2 = "DBNAME2";
        private const string DbName3 = "DBNAME3";

        private const string Code01 = "Code01";
        private const string Code02 = "Code02";
        private const string Code03 = "Code03";
        private const string Code04 = "Code04";
        private const string Code05 = "Code05";

        private const string EquipmentCode1 = "EquipmentSystem_Code01";
        private const string EquipmentCode2 = "EquipmentSystem_Code02";
        private const string EquipmentCode3 = "EquipmentSystem_Code03";
        private const string EquipmentCode4 = "EquipmentSystem_Code04";
        private const string EquipmentCode5 = "EquipmentSystem_Code05";
        
        #endregion

        #region InitializationAndSetup
        [TestInitialize]
        public void TestInitialize()
        {
            MemoryCacheOptions cachingOptions = new MemoryCacheOptions();
            _memoryCache = new MemoryCache(cachingOptions);
            _mockProvider = new MockRepository(MockBehavior.Loose);
            InitStandardConfig();
            _dataParser = new DataParser();
            InitApiImplementationObject();

            //We have to reset this array to ensure no test specific state
            //is carried forward.
            Array.Clear(_defaultMappings,0, _defaultMappings.Length);

            _defaultMappings[0] = new InfluxDBMapping()
            {
                BrandCode = "BrandCode1",
                BrandName = "BrandName1",
                CreatedBy = "Test1",
                CreatedDate = DateTime.Today,
                ModifiedBy = "",
                DbName = DbName1,
                Id = "1",
                MeasurementName = MeasurementName,
                EquipmentCodes = new List<string>() { EquipmentCode1 },
                TechnologyCode = "TechnolgyCode1",
                TechnologyName = "TechnologyName1",
                Status = InfluxDBStatus.Enabled
            };
            _defaultMappings[1] = new InfluxDBMapping()
            {
                BrandCode = "BrandCode2",
                BrandName = "BrandName2",
                CreatedBy = "Test2",
                CreatedDate = DateTime.Today,
                ModifiedBy = "",
                DbName = DbName2,
                Id = "1",
                MeasurementName = MeasurementName,
                EquipmentCodes = new List<string>() { EquipmentCode2 },
                TechnologyCode = "TechnolgyCode2",
                TechnologyName = "TechnologyName2",
                Status = InfluxDBStatus.Enabled
            };
            _defaultMappings[2] = new InfluxDBMapping()
            {
                BrandCode = "BrandCode3",
                BrandName = "BrandName3",
                CreatedBy = "Test1",
                CreatedDate = DateTime.Today,
                ModifiedBy = "",
                DbName = DbName3,
                Id = "1",
                MeasurementName = MeasurementName,
                EquipmentCodes = new List<string>() { EquipmentCode3 },
                TechnologyCode = "TechnologyCode3",
                TechnologyName = "TechnologyName3",
                Status = InfluxDBStatus.Enabled
            };
            _defaultMappings[3] = new InfluxDBMapping()
            {
                BrandCode = "BrandCode4",
                BrandName = "BrandName4",
                CreatedBy = "Test1",
                CreatedDate = DateTime.Today,
                ModifiedBy = "",
                DbName = "DBNAME4",
                Id = "1",
                MeasurementName = MeasurementName,
                EquipmentCodes = new List<string>() { EquipmentCode4 },
                TechnologyCode = "TechnologyCode4",
                TechnologyName = "TechnologyName4",
                Status = InfluxDBStatus.Enabled
            };
            _defaultMappings[4] = new InfluxDBMapping()
            {
                BrandCode = "BrandCode5",
                BrandName = "BrandName5",
                CreatedBy = "Test1",
                CreatedDate = DateTime.Today,
                ModifiedBy = "",
                DbName = "DBNAME5",
                Id = "1",
                MeasurementName = MeasurementName,
                EquipmentCodes = new List<string>() { EquipmentCode5 },
                TechnologyCode = "TechnologyCode5",
                TechnologyName = "TechnologyName5",
                Status = InfluxDBStatus.Enabled
            };

        }

        [TestCleanup]
        public void Cleanup()
        {
            _mockProvider = null;
            _apiImplementation = null;
            _dataParser = null;
            _dataMapper = null;
            _timeStampParser = null;
            _sut = null;
            _channelDefinitionService = null;
        }

        private void InitStandardConfig()
        {
            _ehcApiConfig = new EhcApiConfig();

            var epicV3Api = new ExternalApi();
            InitServiceConfig(ref epicV3Api, "EpicV3Api");
            var equipmentModelApiConfig = new ExternalApi();
            InitServiceConfig(ref equipmentModelApiConfig, "EquipmentModelApiConfig");
            var influxDb = new ExternalApi();
            InitServiceConfig(ref influxDb, "InfluxDb");
            var odmApi = new ExternalApi();
            InitServiceConfig(ref odmApi, "OdmApi");
            var equipmentApi = new ExternalApi();
            InitServiceConfig(ref equipmentApi, "EquipmentApi");

            _ehcApiConfig.EpicV3Api = epicV3Api;
            _ehcApiConfig.EquipmentApi = equipmentApi;
            _ehcApiConfig.EquipmentModelApi = equipmentModelApiConfig;
            _ehcApiConfig.InfluxDB = influxDb;
            _ehcApiConfig.OdmApi = odmApi;
            _ehcApiConfig.ServiceCacheTimeDuration = 24;
        }

        private void InitServiceConfig(ref ExternalApi serviceConfigType, string serviceName)
        {
            serviceConfigType.Username = serviceName + "User";
            serviceConfigType.Password = serviceName + "Password";
            serviceConfigType.TokenAddress = serviceName + "TokenAddress";
            serviceConfigType.TokenCachingInMinutes = 1;
            serviceConfigType.TokenClientId = serviceName + "TokenClientId";
            serviceConfigType.TokenScope = serviceName + "TokenScope";
            serviceConfigType.XApiKey = serviceName + "XapiKey";
            serviceConfigType.TokenClientSecret = serviceName + "TokenClientSecret";
            serviceConfigType.BaseUrl = "https://localhost/mock"+serviceName;
            serviceConfigType.CacheTimeDuration = 24;
        }

        void InitApiImplementationObject()
        {
            _mockEquipmentModelClient = _mockProvider.Create<IEquipmentModelApiClient>();

            ChannelDefinition[] channelDefinitionList = new ChannelDefinition[4];
            channelDefinitionList[0] = new ChannelDefinition()
            {
                Code = FieldNameSiteId,
                Dimension = "ratio",
                Uom = "unitless"
            };
            channelDefinitionList[1] = new ChannelDefinition()
            {
                Code = "time",
                Dimension = "time",
                Uom = "d"
            };
            channelDefinitionList[2] = new ChannelDefinition()
            {
                Code = "Rate",
                Dimension = "pumprate",
                Uom = "BarrelsPerMin"
            };
            channelDefinitionList[3] = new ChannelDefinition()
            {
                Code = "DischargePressure",
                Dimension = "DischargeRate",
                Uom = "DischargesPerMinute"
            };

            _mockChannelDefinitionRepositoryHandler = _mockProvider.Create<IRepositoryHandler<ChannelDefinition>>();
            ConfigureRepositoryHandlerMockToReturnValidChannelDefinition(channelDefinitionList);
            _channelDefinitionService = new ChannelDefinitionService(_mockChannelDefinitionRepositoryHandler.Object, _memoryCache,_ehcApiConfig,_epicV3HierarchyProvider);
            _urlBuilder = new UrlBuilder(_ehcApiConfig);
        }

        private void SetupMocksAndDataForGetChannels()
        {
            var clientHandlerStub = new DelegatingHandlerStub((request, cancellationToken) => {
                var response = new HttpResponseMessage(statusCode: HttpStatusCode.OK);
                response.Content = new StringContent(ReadJson("influxqueryresponse1.json"));
                return Task.FromResult(response);
            });
            ConfigureStandardHttpClientFactoryMock(clientHandlerStub);

            _historianClient = new HistorianClient(_mockHttpClientFactory.Object, _urlBuilder, _ehcApiConfig);

            _autofacBuilder = new ContainerBuilder();
            var responseResolverParams = new List<Parameter>();
            responseResolverParams.Add(new NamedParameter("historianClient", _historianClient));
            responseResolverParams.Add(new NamedParameter("channelDefinitionService", _channelDefinitionService));
            _autofacBuilder.RegisterType<ResponseProviderSingleChannel>().AsSelf().SingleInstance();
            _autofacBuilder.RegisterType<ResponseProviderMultipleChannels>().AsSelf().SingleInstance()
                .WithParameters(responseResolverParams);
            _autofacBuilder.RegisterType<ResponseProviderV1>().AsSelf().SingleInstance();
            _autofacBuilder.RegisterType<ResponseProviderInflux>().AsSelf().SingleInstance();
            _autofacBuilder.RegisterType<ResponseProviderCsv>().AsSelf().SingleInstance();
            _autofacContainer = _autofacBuilder.Build();

            _responseProviderResolver = new ResponseProviderResolver(_autofacContainer);
            _hyperLinksProvider = new HyperLinksProvider();

            _mockEpisodeRepositoryHandler = _mockProvider.Create<IRepositoryHandler<Episode>>();
            _episodeService = new EpisodeService(_mockEpisodeRepositoryHandler.Object);

            _mockUrlBuilder = _mockProvider.Create<IUrlBuilder>();

            //Setup mock expectations and data.
            TLM.EHC.Common.Clients.EquipmentApi.Equipment[] equipments = new TLM.EHC.Common.Clients.EquipmentApi.Equipment[5];
            equipments[0] = GetTestEquipment("1", "1", Code01);   //Uc1DeviceId1

            _mockEquipmentApiClient = new MockEquipmentApiClientService().ConfigureMockForGetEquipmentByWkeIdTracked(equipments);
            _equipmentProvider = new MateoEquipmentProvider(_mockEquipmentApiClient.Object, _memoryCache,_ehcApiConfig);

            
            DynamicInfluxRow[] influxRows = new DynamicInfluxRow[3];
            InfluxPath influxPath = new InfluxPath(); string suffix = "";
            _historianWriter = new MockHistorianWriter().ConfigureMockForWriteData(influxRows, influxPath, suffix);

            _ehcApiConfig.EhcSupportEmail = EhcConstants.EhcSupportEmailId;

            _apiImplementation = new ApiImplementation(_equipmentProvider, _epicV3HierarchyProvider, _responseProviderResolver,
                _historianClient, _hyperLinksProvider, _historianWriter.Object, _episodeService, _channelDefinitionService,_ehcApiConfig, _influxDbMappingService, _mockUrlBuilder.Object, _timeStampParser);
        }

        private void SetupMocksAndDataForBulkChannelsMapping()
        {
            ConfigureStandardHttpClientFactoryMock();

            _historianClient = new HistorianClient(_mockHttpClientFactory.Object, _urlBuilder, _ehcApiConfig);

            _autofacBuilder = new ContainerBuilder();
            
            _responseProviderResolver = new ResponseProviderResolver(_autofacContainer);
            _hyperLinksProvider = new HyperLinksProvider();

            _mockEpisodeRepositoryHandler = _mockProvider.Create<IRepositoryHandler<Episode>>();
            _episodeService = new EpisodeService(_mockEpisodeRepositoryHandler.Object);

            _mockUrlBuilder = _mockProvider.Create<IUrlBuilder>();

            _channelDefinitionService = new ChannelDefinitionService(_mockChannelDefinitionRepositoryHandler.Object, _memoryCache, _ehcApiConfig, _epicV3HierarchyProvider);
            _timeStampParser = new TimestampParser();
            _dataMapper = new DataMapper(_channelDefinitionService, _timeStampParser);

            TLM.EHC.Common.Clients.EquipmentApi.Equipment[] equipments = new TLM.EHC.Common.Clients.EquipmentApi.Equipment[5];
            equipments[0] = GetTestEquipment("1", "1", Code01);               
            equipments[1] = GetTestEquipment("2", "2", Code02);
            equipments[2] = GetTestEquipment("3", "3", Code03);
            equipments[3] = GetTestEquipment("4", "4", Code04);
            equipments[4] = GetTestEquipment("5", "5", Code05);

            _mockEquipmentApiClient = new MockEquipmentApiClientService().ConfigureMockForGetEquipmentByWkeIdTracked(equipments);
            _equipmentProvider = new MateoEquipmentProvider(_mockEquipmentApiClient.Object, _memoryCache, _ehcApiConfig);

            DynamicInfluxRow[] influxRows = new DynamicInfluxRow[3];
            InfluxPath influxPath = new InfluxPath(); string suffix = "";
            _historianWriter = new MockHistorianWriter().ConfigureMockForWriteData(influxRows, influxPath, suffix);

            _ehcApiConfig.EhcSupportEmail = EhcConstants.EhcSupportEmailId;

            _apiImplementation = new ApiImplementation(_equipmentProvider, _epicV3HierarchyProvider, _responseProviderResolver,
                _historianClient, _hyperLinksProvider, _historianWriter.Object, _episodeService, _channelDefinitionService, _ehcApiConfig, _influxDbMappingService, _mockUrlBuilder.Object, _timeStampParser);
        }
        #endregion  

        #region IntegrationTests
        /// <summary>
        /// This test verifies that -
        /// 1. When multiple channel data with distinct row set, EHC API processes it successfully.
        /// 2. Relevant services are called to retrieve equipment related data.
        /// 3. Correct row set is generated for insertion into influx database.
        /// 4. For each equipment, 1 update call is made through historian service which contains relevant
        /// row data (in our test data there are 3 rows for each equipment).
        /// Refer to SendBulkWithMappings.json for more information.
        /// </summary>
        [TestMethod]
        public void Test_PostBulkChannels_WithMappings_WithOkResponse()
        {
            //Arrange
            //This hardcoded channel data is from our test file.
            //Instead of this hard coding it needs to be converted into reading from test files.
            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(_defaultMappings);
            SetupMocksAndDataForBulkChannelsMapping();

            //Act
             _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser);
            var result = _sut.PostBulkChannels(Get_PostBulkChannel_TestData("SendBulkWithMappings.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));

            //Assert
            //1. Check number of service calls.
            //Channel definition service mock is using expression rather than a parameter, cant compare
            //two expressions for equality. Refer to https://stackoverflow.com/questions/6652662/verify-method-call-with-lambda-expression-moq
            _mockChannelDefinitionRepositoryHandler.Verify(s=>s.GetAsync(
                It.Is<Expression<Func<ChannelDefinition, bool>>>(
                    c=>c.Parameters.Count == 1)), Times.Exactly(4), "ChannelDefinitionRepositoryHandler execution count check failed.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId1), Times.Exactly(1));
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId2), Times.Exactly(1));
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId3), Times.Exactly(1));
            _mockEquipmentApiClient.VerifyNoOtherCalls();
            //2. Verify the rows being inserted. If any of the following checks are failing first verify
            //the verification data against the contents of SendBulkWithMappings.json file.
            _historianWriter.Verify(h=>h.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(), 
                It.IsAny<string>()), Times.Exactly(3), "HistorianWriter execution count check failed");
            //TODO : Extract this ugly looking repeating sequence into a function.
            //3 rows for DeviceID141
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734863" && s[0].Fields[FieldNameSiteId].ToString() == SiteId4), 
                It.Is<InfluxPath>(i=>i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s=>s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734863" && s[1].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265137" && s[2].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 141 - WKEID H730200:091");
            //3 rows for DeviceID142
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "7244"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734863" && s[0].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "7248"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734863" && s[1].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7347"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265137" && s[2].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 142 - WKEID H730200:116");
            //3 rows for DeviceID143
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "7161"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734863" && s[0].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 143 - WKEID H730200:233");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "7165"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734863" && s[1].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 143 - WKEID H730200:233");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7264"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265137" && s[2].Fields[FieldNameSiteId].ToString() == SiteId4),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 143 - WKEID H730200:233");
            _historianWriter.VerifyNoOtherCalls();
            _mockChannelDefinitionRepositoryHandler.VerifyAll();
            _mockEquipmentApiClient.VerifyAll();
            _mockEquipmentModelClient.VerifyAll();
            //_mockEpicV3ApiClient.VerifyAll();
            _mockHttpClientFactory.VerifyAll();
        }

        [TestMethod]
        public void Test_PostBulkChannels_WithDisabledDbMap_ThrowsCorrectErrorMessage()
        {
            _mockEpicV3ApiClient = _mockProvider.Create<IEpicV3ApiClient>();
            _epicV3HierarchyProvider = new EpicV3HierarchyProvider(_mockEpicV3ApiClient.Object, _memoryCache, _ehcApiConfig);
            //Arrange
            //This hardcoded channel data is from our test file.
            //Instead of this hard coding it needs to be converted into reading from test files.
            _defaultMappings[2].Status = InfluxDBStatus.Disabled;

            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(_defaultMappings);
            SetupMocksAndDataForBulkChannelsMapping();

            //Act
            _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser);
            var result = _sut.PostBulkChannels(Get_PostBulkChannel_TestData("duplicatechanneldata_UC1_ALL.json"));
            AssertHttpCode<BadRequestObjectResult>(result.Result, 400, String.Format(EhcConstants.InactiveDbMap, EhcConstants.EhcSupportEmailId, "Code03", "EquipmentSystem_Code03"));

            //Assert
            //1. Check number of service calls.
            //Channel definition service mock is using expression rather than a parameter, cant compare
            //two expressions for equality. Refer to https://stackoverflow.com/questions/6652662/verify-method-call-with-lambda-expression-moq
            _mockChannelDefinitionRepositoryHandler.Verify(s => s.GetAsync(
                It.Is<Expression<Func<ChannelDefinition, bool>>>(
                    c => c.Parameters.Count == 1)), Times.Exactly(4), "ChannelDefinitionRepositoryHandler execution count check failed.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId1), Times.Exactly(1));
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId2), Times.Exactly(1));
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId3), Times.Exactly(1));
            _mockEquipmentApiClient.VerifyNoOtherCalls();
            //2. Verify the rows being inserted. If any of the following checks are failing first verify
            //the verification data against the contents of SendBulkWithMappings.json file.
            _historianWriter.Verify(h => h.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(),
                It.IsAny<string>()), Times.Exactly(0), "HistorianWriter execution count check failed");
            _historianWriter.VerifyNoOtherCalls();
            _mockChannelDefinitionRepositoryHandler.VerifyAll();
            _mockEquipmentApiClient.VerifyAll();
            _mockEquipmentModelClient.VerifyAll();
            _mockEpicV3ApiClient.VerifyAll();
            _mockHttpClientFactory.VerifyAll();
        }

        [TestMethod]
        public void Test_PostBulkChannels_WithEquipmentCodeNotInMap_CreatesDbWithDisabledStatus()
        {
            //Arrange
            //This hardcoded channel data is from our test file.
            //Instead of this hard coding it needs to be converted into reading from test files.
            Array.Clear(_defaultMappings,0, _defaultMappings.Length);
            //We also need to configure the epichierarchy provider.
            //_epicV3HierarchyProvider
            _mockEpicV3ApiClient = _mockProvider.Create<IEpicV3ApiClient>();
            _mockEpicV3ApiClient.Setup(m => m.GetEpicHierarchyInfoFromCode("5:Technology_Code01")).Returns(Task.FromResult(
                new EpicRepresentationV3()
                    {
                        Id = "Id1",
                        Attributes = new List<AttributeValue>(),
                        Children = new List<EpicRepresentationV3>(){new EpicRepresentationV3(){Attributes = new List<AttributeValue>(), Children = new List<EpicRepresentationV3>(), Code = "Technology_Code01",
                            CreatedBy = "TestData",
                            CreatedDate = DateTime.Today,
                            ModifiedBy = "TestData",
                            ModifiedDate = DateTime.Today,
                            Name = "Technology1",
                            ParentCode = "Code01",
                            Type = EquipmentClassificationTypeV3.Technology
                        }, new EpicRepresentationV3(){Attributes = new List<AttributeValue>(), Children = new List<EpicRepresentationV3>(), Code = "Brand_Code01",
                            CreatedBy = "TestData",
                            CreatedDate = DateTime.Today,
                            ModifiedBy = "TestData",
                            ModifiedDate = DateTime.Today,
                            Name = "Brand1",
                            ParentCode = "Code01",
                            Type = EquipmentClassificationTypeV3.Brand
                        }
                    },
                        Code = "Code01",
                        CreatedBy = "TestDataHierarchy1",
                        CreatedDate = DateTime.Today,
                        ModifiedBy = "TestDataHierarchy1",
                        ModifiedDate = DateTime.Today,
                        Name = "HierarchyName1",
                        ParentCode = "5:Code01",
                        Type = EquipmentClassificationTypeV3.Technology
                    }));
            _epicV3HierarchyProvider = new EpicV3HierarchyProvider(_mockEpicV3ApiClient.Object, _memoryCache, _ehcApiConfig);

            int repositoryCallTracker = 0;
            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _mockInfluxDbMappingRepositoryHandler.Setup(m => m.UpdateAsync(It.IsAny<InfluxDBMapping>())).Callback(
                (InfluxDBMapping mapping) =>
                {
                    if (mapping == null)
                        throw new ArgumentException("Cannot create DBMap - Incorrect arguments data.");
                    mapping.Id = "returnMappingId";
                    repositoryCallTracker++;
                }).Returns(() => Task.FromResult(true));
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(new InfluxDBMapping[]{});

            SetupMocksAndDataForBulkChannelsMapping();

            //Act
            _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser);
            var result = _sut.PostBulkChannels(Get_PostBulkChannel_TestData("duplicatechanneldata_UC1_ALL.json"));
            AssertHttpCode<BadRequestObjectResult>(result.Result, 400, String.Format(EhcConstants.InactiveDbMap, EhcConstants.EhcSupportEmailId, "Code01", "EquipmentSystem_Code01"));

            //Assert
            //1. Check number of service calls.
            //Channel definition service mock is using expression rather than a parameter, cant compare
            //two expressions for equality. Refer to https://stackoverflow.com/questions/6652662/verify-method-call-with-lambda-expression-moq
            _mockChannelDefinitionRepositoryHandler.Verify(s => s.GetAsync(
                It.Is<Expression<Func<ChannelDefinition, bool>>>(
                    c => c.Parameters.Count == 1)), Times.Exactly(4), "ChannelDefinitionRepositoryHandler execution count check failed.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId1), Times.Exactly(1));
            _mockEquipmentApiClient.VerifyNoOtherCalls();
            //2. Verify the rows being inserted. If any of the following checks are failing first verify
            //the verification data against the contents of SendBulkWithMappings.json file.
            _historianWriter.Verify(h => h.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(),
                It.IsAny<string>()), Times.Exactly(0), "HistorianWriter execution count check failed");
            Assert.IsTrue(repositoryCallTracker == 1,"Expected only one call to InfluxDBRepository but found " + Convert.ToString(repositoryCallTracker));
            _historianWriter.VerifyNoOtherCalls();
            _mockChannelDefinitionRepositoryHandler.VerifyAll();
            _mockEquipmentApiClient.VerifyAll();
            _mockEquipmentModelClient.VerifyAll();
            _mockEpicV3ApiClient.VerifyAll();
            _mockHttpClientFactory.VerifyAll();
        }

        private void AssertHttpCode<T>(IActionResult actual, int httpCode, string expectedMessage) where T : ObjectResult
        {
            Assert.IsNotNull(actual);
            T objectResult = actual as T;
            Debug.Assert(objectResult != null, nameof(objectResult) + " != null");
            Assert.AreEqual(objectResult.StatusCode, httpCode);
            Tlm.Sdk.Core.Models.Hypermedia.Error err = (Tlm.Sdk.Core.Models.Hypermedia.Error)objectResult.Value;
            Assert.IsTrue(String.Compare(expectedMessage, err.Detail, StringComparison.Ordinal) == 0, "Error message does not match with expected value.");
        }


        /// <summary>
        /// This test verifies that -
        /// 1. When multiple channel data with single rowset and ALL tag, EHC API processes it successfully.
        /// 2. Relevant services are called to retrieve equipment related data.
        /// 3. Correct row set is generated for insertion into influx database.
        /// 4. For each equipment, 1 update call is made through historian service which contains relevant
        /// row data (in our test data there are 3 rows for each equipment).
        /// 5. Note that same data is getting copied for all equipments.
        /// Refer to duplicatechanneldata_UC1_ALL.json for more information.
        /// </summary>
        [TestMethod]
        public void Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse()
        {
            //Arrange
            _mockEpicV3ApiClient = _mockProvider.Create<IEpicV3ApiClient>();
            _epicV3HierarchyProvider = new EpicV3HierarchyProvider(_mockEpicV3ApiClient.Object, _memoryCache, _ehcApiConfig);
            //This hardcoded channel data is from our test file.
            //Instead of this hard coding it needs to be converted into reading from test files.
            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(_defaultMappings);
            SetupMocksAndDataForBulkChannelsMapping();

            //Act
            _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser);
            var result = _sut.PostBulkChannels(Get_PostBulkChannel_TestData("duplicatechanneldata_UC1_ALL.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));
            //1. Check number of service calls.
            _mockChannelDefinitionRepositoryHandler.Verify(s => s.GetAsync(
                It.Is<Expression<Func<ChannelDefinition, bool>>>(
                    c => c.Parameters.Count == 1)), Times.Exactly(4),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - ChannelDefinitionRepositoryHandler execution count check failed.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId1), Times.Exactly(1), 
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:091 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId2), Times.Exactly(1), 
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:116 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId3), Times.Exactly(1), 
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:233 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.VerifyNoOtherCalls();
            //2. Verify the rows being inserted. If any of the following checks are failing first verify
            //the verification data against the contents of SendBulkWithMappings.json file.
            _historianWriter.Verify(h => h.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(),
                It.IsAny<string>()), Times.Exactly(3), "HistorianWriter execution count check failed");
            //TODO : Extract this ugly looking repeating sequence into a function.
            //3 rows for DeviceID141
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 141 - WKEID H730200:091");
            //3 rows for DeviceID142
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 142 - WKEID H730200:116");
            ////3 rows for DeviceID143
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 143 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 143 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 143 - WKEID H730200:091");
            _historianWriter.VerifyNoOtherCalls();

            _mockChannelDefinitionRepositoryHandler.VerifyAll();
            _mockEquipmentApiClient.VerifyAll();
            _mockEquipmentModelClient.VerifyAll();
            _mockEpicV3ApiClient.VerifyAll();
            _mockHttpClientFactory.VerifyAll();
        }

        /// <summary>
        /// This test verifies that -
        /// 1. When multiple channel data with -
        /// 1.1 single rowset is provided for all equipments
        /// 1.2 unique rowset is provided for some equipments
        /// 1.3 common rowset should get updated for all equipments but unique rowset should be updated
        /// only for the relevant equipments
        /// 2. Relevant services are called to retrieve equipment related data.
        /// 3. Correct row set is generated for insertion into influx database.
        /// 4. For each equipment, 1 update call is made through historian service which contains relevant
        /// row data (in our test data there are 3 rows for each equipment).
        /// Refer to duplicatechanneldata_UC3_Mixed.json for more information.
        /// </summary>
        [TestMethod]
        public void Test_PostBulkChannels_WithMappings_WithMixedMode_WithOkResponse()
        {
            //Arrange
            _mockEpicV3ApiClient = _mockProvider.Create<IEpicV3ApiClient>();
            _epicV3HierarchyProvider = new EpicV3HierarchyProvider(_mockEpicV3ApiClient.Object, _memoryCache, _ehcApiConfig);
            //This hardcoded channel data is from our test file.
            //Instead of this hard coding it needs to be converted into reading from test files.
            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(_defaultMappings);
            SetupMocksAndDataForBulkChannelsMapping();

            //Act
            _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser);
            var result = _sut.PostBulkChannels(Get_PostBulkChannel_TestData("duplicatechanneldata_UC3_Mixed.json"));
            (result.Result).Should().NotBeNull();
            Assert.IsInstanceOfType(result.Result, typeof(OkResult));
            //Assert
            //1. Check number of service calls.
            _mockChannelDefinitionRepositoryHandler.Verify(s => s.GetAsync(
                    It.Is<Expression<Func<ChannelDefinition, bool>>>(
                        c => c.Parameters.Count == 1)), Times.Exactly(4),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - ChannelDefinitionRepositoryHandler execution count check failed.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId1), Times.Exactly(1),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:091 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId2), Times.Exactly(1),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:116 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId(Uc1DeviceId3), Times.Exactly(1),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:233 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId("H730201:233"), Times.Exactly(1),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:233 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.Verify(e => e.GetEquipmentByWkeId("H730202:233"), Times.Exactly(1),
                "Test_PostBulkChannels_WithMappings_WithAllTag_WithOkResponse - H730200:233 - failed when verifying GetEquipmentByWkeId call.");
            _mockEquipmentApiClient.VerifyNoOtherCalls();
            //2. Verify the rows being inserted. If any of the following checks are failing first verify
            //the verification data against the contents of SendBulkWithMappings.json file.
            _historianWriter.Verify(h => h.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(),
                It.IsAny<string>()), Times.Exactly(5), "HistorianWriter execution count check failed");
            //TODO : Extract this ugly looking repeating sequence into a function.
            //6 rows for DeviceID141
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 141 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode1 && i.Brand == MeasurementName && i.Technology == DbName1),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 141 - WKEID H730200:091");

            //Uncommenting this leads to JArray.ToObject bug in DataParser.cs - Line no 168.
            //_historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "16984"
            //        && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734444" && s[0].Fields[FieldNameSiteId].ToString() == "PC-RCAVE-02:6ID10"),
            //    It.Is<InfluxPath>(i => i.EquipmentCode == Code01 && i.Brand == MeasurementName && i.Technology == DbName1),
            
#pragma warning disable S125 // Sections of code should not be commented out
//    It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 3 deviceID 141 - WKEID H730200:091");

            //3 rows for DeviceID142
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
#pragma warning restore S125 // Sections of code should not be commented out
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 142 - WKEID H730200:116");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode2 && i.Brand == MeasurementName && i.Technology == DbName2),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 142 - WKEID H730200:116");
            ////3 rows for DeviceID143
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[0].Fields[FieldNameDischargeRate].ToString() == "6984"
                    && s[0].Fields[BarrelsPerMinRate].ToString() == "5.800000190734111" && s[0].Fields[FieldNameSiteId].ToString() == SiteId1),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 0 deviceID 143 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[1].Fields[FieldNameDischargeRate].ToString() == "6988"
                    && s[1].Fields[BarrelsPerMinRate].ToString() == "4.300000190734222" && s[1].Fields[FieldNameSiteId].ToString() == SiteId2),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 1 deviceID 143 - WKEID H730200:091");
            _historianWriter.Verify(h => h.WriteData(It.Is<DynamicInfluxRow[]>(s => s[2].Fields[FieldNameDischargeRate].ToString() == "7087"
                    && s[2].Fields[BarrelsPerMinRate].ToString() == "6.199999809265333" && s[2].Fields[FieldNameSiteId].ToString() == SiteId3),
                It.Is<InfluxPath>(i => i.EquipmentCode == EquipmentCode3 && i.Brand == MeasurementName && i.Technology == DbName3),
                It.Is<string>(s => s.Length == 0)), Times.Exactly(1), "HistorianWriter data update verification failed for row 2 deviceID 143 - WKEID H730200:091");
            _historianWriter.VerifyNoOtherCalls();
        }


        [TestMethod]
        public async Task Test_GetChannels_Returns_Relevant_RowsAsync()
        {
            //Arrange
            _timeStampParser = new TimestampParser();
            InfluxDBMapping[] mappings = new[]
            {
                new InfluxDBMapping()
                {
                    BrandCode = "BrandCode1",
                    BrandName = "BrandName1",
                    CreatedBy = "Test1",
                    CreatedDate = DateTime.Today,
                    ModifiedBy = "",
                    DbName = DbName1,
                    Id = "1",
                    MeasurementName = MeasurementName,
                    EquipmentCodes = new List<string>(){EquipmentCode1},
                    TechnologyCode = "TechnolgyCode1",
                    TechnologyName = "TechnologyName1",
                    Status = InfluxDBStatus.Enabled
                }
            };
            _mockInfluxDbMappingRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _influxDbMappingService = new InfluxDBMappingService(_mockInfluxDbMappingRepositoryHandler.Object, _memoryCache, _ehcApiConfig);
            ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(mappings);

            SetupMocksAndDataForGetChannels();
            string queryEquipmentId = Uc1DeviceId1;
            string start = DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture);
            string end = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            string codes = "codes";
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["accept"] = "application/json";

            _sut = new ChannelsController(_apiImplementation, _dataParser, _dataMapper, _timeStampParser)
                { ControllerContext = new ControllerContext(){HttpContext = httpContext} };
            
            //Act
            var result = await _sut.GetChannels(queryEquipmentId, start, end, codes);
            //Assert
            Assert.IsTrue(result != null);
            Assert.IsInstanceOfType(result,( typeof(ActionResult)));
        }

        /// <summary>
        /// Reads test input data from es-TLM-EHC-API\EHC.API\JSON\ChannelData directory, returns JToken
        /// object which can be used to target the test API.
        /// </summary>
        /// <param name="fileName">Target file to read.</param>
        /// <returns>Reads JToken representing json structure being read.</returns>
        private JToken Get_PostBulkChannel_TestData(string fileName)
        {
            return JToken.Parse(File.ReadAllText(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "../../../../EHC.API/JSON/ChannelData/Input", fileName))));
        }

        /// <summary>
        /// Reads test data from es-TLM-EHC-API\EHC.API\JSON\ChannelData directory, returns string data
        /// which can be used to configure test output for mock objects (e.g. dummy response from InfluxAPI
        /// to simulate HttpClient call to Influx API).
        /// </summary>
        /// <param name="fileName">Target file to read.</param>
        /// <returns>String data read from target file.</returns>
        private string ReadJson(string fileName)
        {
            return File.ReadAllText(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(),
                "../../../../EHC.API/JSON/ChannelData", fileName)));
        }
        #endregion

        

        #region MockConfiguration
        private void ConfigureStandardHttpClientFactoryMock(DelegatingHandlerStub handlerStub = null)
        {
            _mockHttpClientFactory = _mockProvider.Create<IHttpClientFactory>();
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            handlerStub ??= new DelegatingHandlerStub();
            _client = new HttpClient(handlerStub);
            _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(_client);

        }

        #region RepositoryMocks
        /// <summary>
        /// Configuring repository handler is not following the pattern we have implemented with other mocks (refer to mocks defined in
        /// Mocks directory). I have attempted converting this mock into a generic one, however generating the abstract linq expression
        /// for all types turned out to be too complicated. Leaving these type specific repository mock configurations in place for now.
        /// Why is this mock special? : Because we are running an expression on set of channel definitions AFTER we feed them to the
        /// mock in case of channel definitions. For other repository objects we are feeding specific response to be returned. For channel
        /// data its a collection of channels and mock processes one collection at a time, hence the filtering.
        /// </summary>
        /// <param name="channelDefinitions"></param>
        private void ConfigureRepositoryHandlerMockToReturnValidChannelDefinition(ChannelDefinition[] channelDefinitions)
        {
            _mockChannelDefinitionRepositoryHandler.Setup(m =>
                    m.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(((Expression<Func<ChannelDefinition, bool>> predicate) =>
                    Task.FromResult(channelDefinitions.Where(predicate.Compile()).ToList())))
                .Verifiable("ChannelDefinition retrieval call was expected but was not performed.");

        }

        /// <summary>
        /// This configures the mock to return same set of mapping values. For a more extended set of tests we will
        /// need a custom mock that will iterate through set of values and return a different set corresponding to
        /// the input value.
        /// </summary>
        /// <param name="mappings"></param>
        private void ConfigureRepositoryHandlerMockToReturnValidInfluxMapping(InfluxDBMapping[] mappings)
        {
            if (mappings.Length > 0)
            {
                _mockInfluxDbMappingRepositoryHandler
                    .Setup(i => i.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                    //.Returns(Task.FromResult(mappings.ToList()));                 //The shortcut: Just return same response irrespective of input
                    .Returns((Expression<Func<InfluxDBMapping, bool>> predicate) =>
                        Task.FromResult(mappings.Where(predicate.Compile()).ToList())
                    ); //This returns more meaningful response by matching the EquipmentCode from input to mappings array.
            }
            else
            {
                _mockInfluxDbMappingRepositoryHandler
                    .Setup(i => i.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                    //.Returns(Task.FromResult(mappings.ToList()));                 //The shortcut: Just return same response irrespective of input
                    .Returns((Expression<Func<InfluxDBMapping, bool>> predicate) =>
                        Task.FromResult((new List<InfluxDBMapping>()))
                    ); //This returns more meaningful response by matching the EquipmentCode from input to mappings array.
            }

        }
        #endregion

        private TLM.EHC.Common.Clients.EquipmentApi.Equipment GetTestEquipment(string postFixForTestData, string equipmentId, string equipmentCode)
        {
            TLM.EHC.Common.Clients.EquipmentApi.Equipment equipment = new TLM.EHC.Common.Clients.EquipmentApi.Equipment()
            {
                ActiveCmms = "CMMS" + postFixForTestData,
                AlternateIdentities = null,
                AssetNumber = "Asset" + postFixForTestData,
                Attributes = null,
                ChildEquipment = null,
                Classifications = new Dictionary<string, ICollection<Classification>>()
                {
                    {
                        "key1", new List<Classification> {
                            new Classification(){ Code = "EquipmentSystem_" + equipmentCode, ParentCode = "ParentCode1", Type = "EquipmentSystem" },
                            new Classification(){ Code = "Brand_" + equipmentCode, ParentCode = "ParentCode1", Type = "Brand" },
                            new Classification(){ Code = "Technology_" + equipmentCode, ParentCode = "ParentCode1", Type = "Technology" }
                        }
                    },
                },
                Comments = null,
                ControlSiteCode = "ControlSiteCode" + postFixForTestData,
                ControlSiteId = "ControlSiteId" + postFixForTestData,
                CountryOfOrigin = "Country" + postFixForTestData,
                CreatedBy = "CreatedBy" + postFixForTestData,
                CreatedDate = DateTime.Today,
                Description = "Description" + postFixForTestData,
                EquipmentCode = equipmentCode,
                EquipmentDemandSummary = null,
                EquipmentState = "EquipmentState" + postFixForTestData,
                EquipmentStatus = null,
                EquipmentType = EquipmentType.Equipment,
                FutureMaintenanceCalls = null,
                Id = equipmentId,
                Location = null,
                ManufacturedDate = DateTime.Today,
                Manufacturer = "Manufacturer" + postFixForTestData,
                ManufacturerSerialNumber = "ManufacturerSerialNumber" + postFixForTestData,
                ManufacturersCode = "ManufacturersCode" + postFixForTestData,
                MaterialNumber = "MaterialSerialNumber" + postFixForTestData,
                MeasurementPoints = null,
                ModifiedBy = "ModifiedBy" + postFixForTestData,
                ModifiedDate = DateTime.Today,
                MovementSummary = null,
                OrderLineId = "OrderLineId" + postFixForTestData,
                OriginationDate = DateTime.Today,
                Owner = "Owner" + postFixForTestData,
                OwnerSiteCode = "OwnerSiteCode" + postFixForTestData,
                OwnerSiteId = "OwnerSiteId" + postFixForTestData,
                Ownership = Ownership.SLB,
                PairedEquipment = null,
                PairedEquipmentId = "",
                ParentEquipment = null,
                ParentEquipmentId = "",
                Position = null,
                Revision = "",
                SerialNumber = "SerialNumber",
                SourceSystemRecordId = "",
                UnmanagedAttributes = null,
                WellKnownEntityId = equipmentCode,
                Workorders = null
            };
            return equipment;
        }
        #endregion

    }
}
