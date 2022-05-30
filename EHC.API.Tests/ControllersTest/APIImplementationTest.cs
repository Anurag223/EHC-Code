using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using EHC.API.Tests.Mocks;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.Admin;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.HyperLinks;
using TLM.EHC.API.Models;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common;
using TLM.EHC.Common.Clients.EquipmentApi;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using Vibrant.InfluxDB.Client.Rows;
using Equipment = TLM.EHC.Common.Models.Equipment;
using EquipmentModel = TLM.EHC.Common.Models.EquipmentModel;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class ApiImplementationTest
    {
        Mock<IEquipmentProvider> _mockEquipmentProvider;
        Mock<IEquipmentModelProvider> _mockEquipmentModelProvider;
        Mock<IEpicV3HierarchyProvider> _mockEpicV3HierarchyProvider;
        Mock<IResponseProviderResolver> _mockResponseProviderResolver;
        Mock<IHistorianClient> _mockHistorianClient;
        Mock<IHyperLinksProvider> _mockHyperLinksProvider;
        Mock<IHistorianWriter> _mockHistorianWriter;
        Mock<IEpisodeService> _mockEpisodeService;
        Mock<IChannelDefinitionService> _mockChannelDefinitionService;
        private Mock<IInfluxDBMappingService> _mockInfluxDbMappingService;

        private Equipment _testEquipment;
        private WellKnownEntityId[] _testWkeidArray;
        private Equipment[] _validEquipments;
        private string[] _codes;
        private EquipmentModel[] _validEquipmentModels;
        EpicV3Hierarchy[] _testHierarchy = new EpicV3Hierarchy[5];
        private EhcApiConfig _apiConfig;
        private Mock<IUrlBuilder> _urlBuilder;
        private Mock<ITimestampParser> _timeStampParser;

        public EhcApiConfig ConfigDetails()
        {
            return new EhcApiConfig
            {
                InfluxDB = new ExternalApi
                {
                    BaseUrl = "http://testInfluxDb.com",
                    Username = "testInflux",
                    Password = "test@123"
                }
            };
        }

        public InfluxResponse GetInfluxResponse()
        {
            return new InfluxResponse
            {
                Results = new[]
                {
                    new InfluxSeries
                    {
                        Series = new[]
                        {
                            new QueryResult
                            {
                                Columns = new List<string> {"DischargeRate"},
                                Tags = new EquipmentInstanceResultSet
                                {
                                    EquipmentInstance = "md1:SPF7432134"
                                },
                                Name = "testInfluxResponse",
                                Values = new List<List<object>>
                                {
                                    new List<object> {"021-09-14T16:30:07.293Z", 0.0871633142232895 }
                                }
                            }
                        }
                    }
                }
            };
        }

        public HistorianClient GetHistorianClient()
        {
            return new HistorianClient(new HttpClientFake(), _urlBuilder.Object, _apiConfig);
        }

        private void InitTestEquipments()
        {
            _validEquipments = new[]
            {
                new Equipment
                {EquipmentCode = _codes[0],
                    EquipmentWkeId = "0",
                    MaterialNumber = _testWkeidArray[0].MaterialNumber,
                    SerialNumber = _testWkeidArray[0].SerialNumber,
                    SourceSystemRecordId = "0",
                    EpicClassifications = new List<Classification>
                    {
                        new Classification
                        {
                            Code="e_code01",
                            ParentCode = "ParentCode",
                            Type = "EquipmentSystem"
                        },
                        new Classification{
                            Code="e_code2",
                            ParentCode = "ParentCode",
                            Type = "Brand"
                        },
                        new Classification{
                            Code="e_code3",
                            ParentCode = "ParentCode",
                            Type = "Technology"
                        }}
                },
                new Equipment
                {EquipmentCode = _codes[1],
                    EquipmentWkeId = "1",
                    MaterialNumber = _testWkeidArray[1].MaterialNumber,
                    SerialNumber = _testWkeidArray[1].SerialNumber,
                    SourceSystemRecordId = "1",
                    EpicClassifications = new List<Classification>
                    {
                        new Classification
                        {
                            Code="e_code04",
                            ParentCode = "ParentCode",
                            Type = "EquipmentSystem"
                        },
                        new Classification{
                            Code="e_code5",
                            ParentCode = "ParentCode",
                            Type = "Brand"
                        },
                        new Classification{
                            Code="e_code6",
                            ParentCode = "ParentCode",
                            Type = "Technology"
                        }}
                },
                new Equipment
                {EquipmentCode = _codes[2],
                    EquipmentWkeId = "2",
                    MaterialNumber = _testWkeidArray[2].MaterialNumber,
                    SerialNumber = _testWkeidArray[2].SerialNumber,
                    SourceSystemRecordId = "2",
                    EpicClassifications = new List<Classification>
                    {
                        new Classification
                        {
                            Code="e_code07",
                            ParentCode = "ParentCode",
                            Type = "EquipmentSystem"
                        },
                        new Classification{
                            Code="e_code8",
                            ParentCode = "ParentCode",
                            Type = "Brand"
                        },
                        new Classification{
                            Code="e_code9",
                            ParentCode = "ParentCode",
                            Type = "Technology"
                        }}
                },
                new Equipment
                {EquipmentCode = _codes[3],
                    EquipmentWkeId = "3",
                    MaterialNumber = _testWkeidArray[3].MaterialNumber,
                    SerialNumber = _testWkeidArray[3].SerialNumber,
                    SourceSystemRecordId = "3",
                    EpicClassifications = new List<Classification>
                    {
                        new Classification
                        {
                            Code="e_code010",
                            ParentCode = "ParentCode",
                            Type = "EquipmentSystem"
                        },
                        new Classification{
                            Code="e_code11",
                            ParentCode = "ParentCode",
                            Type = "Brand"
                        },
                        new Classification{
                            Code="e_code12",
                            ParentCode = "ParentCode",
                            Type = "Technology"
                        }}
                },
                new Equipment
                {EquipmentCode = _codes[4],
                    EquipmentWkeId = "4",
                    MaterialNumber = _testWkeidArray[4].MaterialNumber,
                    SerialNumber = _testWkeidArray[4].SerialNumber,
                    SourceSystemRecordId = "4",
                    EpicClassifications = new List<Classification>
                    {
                        new Classification
                        {
                            Code="e_code013",
                            ParentCode = "ParentCode",
                            Type = "EquipmentSystem"
                        },
                        new Classification{
                            Code="e_code14",
                            ParentCode = "ParentCode",
                            Type = "Brand"
                        },
                        new Classification{
                            Code="e_code15",
                            ParentCode = "ParentCode",
                            Type = "Technology"
                        }}
                }
            };
        }

        private void InitTestEquipmentModels()
        {
            _validEquipmentModels = new[]
            {
                new EquipmentModel
                {
                    BrandCode = "1",
                    BrandName = "BrandOne",
                    Channels = new List<EquipmentModelChannel>
                    {
                        new EquipmentModelChannel { Code="Code1", Dimension = "Dimension1", LegalClassification = "LegalClassification1", Name="Name 1", Uom="Uom1"}
                    },
                    Description = "Description1",
                    MaterialNumber = "1",
                    EquipmentCode = "e_code01",
                    TechnologyCode = "Tech1",
                    TechnologyName = "TechName1"
                },
                new EquipmentModel
                {
                    BrandCode = "2",
                    BrandName = "BrandTwo",
                    Channels = new List<EquipmentModelChannel>
                    {
                        new EquipmentModelChannel { Code="Code2", Dimension = "Dimension2", LegalClassification = "LegalClassification2", Name="Name 2", Uom="Uom2"}
                    },
                    Description = "Description2",
                    MaterialNumber = "2",
                    EquipmentCode = "e_code02",
                    TechnologyCode = "Tech2",
                    TechnologyName = "TechName2"
                },
                new EquipmentModel
                {
                    BrandCode = "3",
                    BrandName = "BrandThree",
                    Channels = new List<EquipmentModelChannel>
                    {
                        new EquipmentModelChannel { Code="Code3", Dimension = "Dimension3", LegalClassification = "LegalClassification3", Name="Name 3", Uom="Uom3"}
                    },
                    Description = "Description3",
                    MaterialNumber = "3",
                    EquipmentCode = "e_code03",
                    TechnologyCode = "Tech3",
                    TechnologyName = "TechName3"
                },
                new EquipmentModel
                {
                    BrandCode = "4",
                    BrandName = "BrandFour",
                    Channels = new List<EquipmentModelChannel>
                    {
                        new EquipmentModelChannel { Code="Code4", Dimension = "Dimension4", LegalClassification = "LegalClassification4", Name="Name 4", Uom="Uom4"}
                    },
                    Description = "Description4",
                    MaterialNumber = "4",
                    EquipmentCode = "e_code04",
                    TechnologyCode = "Tech4",
                    TechnologyName = "TechName4"
                },
                new EquipmentModel
                {
                    BrandCode = "5",
                    BrandName = "BrandFive",
                    Channels = new List<EquipmentModelChannel>
                    {
                        new EquipmentModelChannel { Code="Code5", Dimension = "Dimension5", LegalClassification = "LegalClassification5", Name="Name 5", Uom="Uom5"}
                    },
                    Description = "Description5",
                    MaterialNumber = "5",
                    EquipmentCode = "e_code05",
                    TechnologyCode = "Tech5",
                    TechnologyName = "TechName5"
                }
            };

        }

        [TestInitialize]
        public void InitStandardTestObjectSet()
        {
            _testEquipment = new Equipment
            {
                EquipmentCode = "testequipmentcode",
                EquipmentWkeId = "100196736:SBF62412A0281",
                MaterialNumber = "100196736",
                SerialNumber = "SBF62412A0281",
                SourceSystemRecordId = "SBF62412A0281",
                EpicClassifications = new List<Classification>
                {
                    new Classification
                    {
                        Code="code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _testWkeidArray = new[]
            {
                new WellKnownEntityId(materialNumber:"1", serialNumber:"wkeid1"),
                new WellKnownEntityId(materialNumber:"2", serialNumber:"wkeid2"),
                new WellKnownEntityId(materialNumber:"3", serialNumber:"wkeid3"),
                new WellKnownEntityId(materialNumber:"4", serialNumber:"wkeid4"),
                new WellKnownEntityId(materialNumber:"5", serialNumber:"wkeid5")
            };

            _codes = new[]
            {
                "code 1", "code 2", "code 3", "code 4", "code 5"
            };

            InitTestEquipments();

            InitTestEquipmentModels();

            _testHierarchy = new EpicV3Hierarchy[5];
            _testHierarchy[0] = new EpicV3Hierarchy
            {
                Name = "TestHierarchy01",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code1",
                        Name = "C1"
                    },
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code2",
                        Name = "C2"
                    }
                }
            };
            _testHierarchy[1] = new EpicV3Hierarchy
            {
                Name = "TestHierarchy02",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code5",
                        Name = "C5"
                    }
                }
            };
            _testHierarchy[2] = new EpicV3Hierarchy
            {
                Name = "TestHierarchy03",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code8",
                        Name = "C8"
                    }
                }
            };

            _testHierarchy[3] = new EpicV3Hierarchy
            {
                Name = "TestHierarchy04",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code11",
                        Name = "C11"
                    }
                }
            };

            _testHierarchy[4] = new EpicV3Hierarchy
            {
                Name = "TestHierarchy05",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "e_code14",
                        Name = "C14"
                    }
                }
            };
        }
        
        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public async Task Verify_GetRows_ThrowsInvalidOperationException_for_null_datatypesuffix()
        {
            //Arrange
            var request = new RowsRequest
            {
                EpisodeId = "1", QueryType = QueryType.Unknown
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    null);
            ApiImplementation apiImplementation = GetApiImplementationInstance();

            //Act
            await apiImplementation.GetRows(request);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task Verify_GetRows_ThrowsBadRequestException_for_episodeIdNotFound()
        {
            RowsRequest request = new RowsRequest
            {
                EpisodeId = "1",
                QueryType = QueryType.Unknown,
                DataType = DataType.Episodic
            };
            
            _mockEpisodeService = new MockEpisodeService().ConfigureMockForGetEpisodeById(request.EpisodeId, null);

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            EpicV3Hierarchy testHierarchy = new EpicV3Hierarchy
            {
                Name = "TestHierarchyName",
                Children = new List<EpicV3Hierarchy>
                {
                    new EpicV3Hierarchy
                    {
                        Children = null,
                        Code = "code2",
                        Name = "C1"
                    }
                }
            };
            
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", testHierarchy);

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetRows(request);
        }

        [TestMethod]
        public async Task Verify_GetRows_Calls_GetResponseNoData()
        {
            RowsRequest request = new RowsRequest
            {
                EpisodeId = "1",
                QueryType = QueryType.SingleCode,
                DataType = DataType.Episodic,
                ResponseFormat = ResponseFormat.Influx,
                Codes = _codes
            };
            
            Episode episode = new Episode
            {
                CreatedBy = "testcreator",
                CreatedDate = DateTime.Today.Date.AddDays(-5),
                EndTime = DateTime.Today.Date.AddDays(-1)
            };

            _mockEpisodeService = new MockEpisodeService().ConfigureMockForGetEpisodeById(request.EpisodeId, episode);

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCode(
                    _testEquipment.EquipmentCode, _validEquipmentModels[0]);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<SingleChannel>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            moqResponseProvider.Setup(o => o.GetResponseNoData(It.IsAny<RowsRequest>(), It.IsAny<QueryContext>()))
                .Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetRows(request);

            _mockEpisodeService.Verify();
            _mockEquipmentProvider.Verify();
            _mockEquipmentModelProvider.Verify();
        }

        [TestMethod]
        public async Task Verify_Query_When_Episodic_WithoutTimePeriod()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {
              
                QueryType = QueryType.SingleCode,
                DataType = DataType.Episodic,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[]{code}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
            new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT Episode,\"AirPressure\" FROM \"CODE2_C1.Episodic\" WHERE EquipmentInstance='100196736:SBF62412A0281'";

            query.SelectText.Should().Be(finalQueryResult);

            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        public async Task Verify_Query_When_Episodic_WithTimePeriod()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {

                QueryType = QueryType.SingleCode,
                DataType = DataType.Episodic,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { code },
                TimePeriod = new TimePeriod(new DateTime(2021, 8, 10, 12, 02, 45), new DateTime(2021, 9, 10, 12, 02, 45))
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT Episode,\"AirPressure\" FROM \"CODE2_C1.Episodic\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1628596965000000000 AND time <= 1631275365000000000";

            query.SelectText.Should().Be(finalQueryResult);
            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        public async Task Verify_Query_When_Channel_WithTimePeriod_WithAggregation()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {

                QueryType = QueryType.SingleCode,
                DataType = DataType.Channel,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { code },
                AggregateFunction = AggregationFunctions.Mean,
                GroupbyTimeValue = "1h",
                FillValue = "previous",
                TimePeriod = new TimePeriod(new DateTime(2021, 8, 10, 12, 02, 45), new DateTime(2021, 9, 10, 12, 02, 45))
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT mean(\"AirPressure\") AS \"mean_AirPressure\" FROM \"CODE2_C1\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1628596965000000000 AND time <= 1631275365000000000 GROUP BY time(1h) FILL(previous)";

            query.SelectText.Should().Be(finalQueryResult);
            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        public async Task Verify_Query_When_Channel_WithOutTimePeriod_WithAggregation()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {

                QueryType = QueryType.SingleCode,
                DataType = DataType.Channel,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { code },
                AggregateFunction = AggregationFunctions.Mean,
                GroupbyTimeValue = "1h",
                FillValue = "previous",
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT mean(\"AirPressure\") AS \"mean_AirPressure\" FROM \"CODE2_C1\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1628510565000000000 AND time <= 1628596965000000000 GROUP BY time(1h) FILL(previous)";

            query.SelectText.Should().Be(finalQueryResult);
            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        public async Task Verify_Query_When_Readings_WithOutTimePeriod()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {

                QueryType = QueryType.SingleCode,
                DataType = DataType.Reading,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { code },
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT \"AirPressure\" FROM \"CODE2_C1.Reading\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1628510565000000000 AND time <= 1628596965000000000";

            query.SelectText.Should().Be(finalQueryResult);
            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        public async Task Verify_Query_When_Readings_WithTimePeriod()
        {
            string code = "AirPressure";
            RowsRequest request = new RowsRequest
            {

                QueryType = QueryType.SingleCode,
                DataType = DataType.Reading,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { code },
                TimePeriod = new TimePeriod(new DateTime(2021, 8, 10, 12, 02, 45), new DateTime(2021, 9, 10, 12, 02, 45))
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    _testEquipment);

            DateTime? value = new DateTime(2021, 8, 10, 12, 02, 45);
            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetLatestTimestamp(value);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _testHierarchy[0].Children.Add(new EpicV3Hierarchy
            {
                Children = null,
                Code = "code2",
                Name = "C1"
            });
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider()
                    .ConfigureMockForGetEpicHierarchyInfoFromCode("5:code3", _testHierarchy[0]);

            ApiImplementation apiImplementation = GetApiImplementationInstance();

            _mockChannelDefinitionService.Setup(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(code));

            await apiImplementation.GetRows(request);

            var finalQueryResult =
                "SELECT \"AirPressure\" FROM \"CODE2_C1.Reading\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1628596965000000000 AND time <= 1631275365000000000";

            query.SelectText.Should().Be(finalQueryResult);
            _mockResponseProviderResolver.Verify();
            _mockChannelDefinitionService.Verify();
            _mockEpicV3HierarchyProvider.Verify();
            _mockResponseProviderResolver.Verify();
            moqResponseProvider.Verify();
            _mockEquipmentProvider.Verify();

        }

        [TestMethod]
        [ExpectedException(typeof(ServerErrorException))]
        public async Task Verify_GetRows_Returns_ServerException_WhenEquipmentClassificationNotFound()
        {
            RowsRequest request = new RowsRequest
            {
                EpisodeId = "1",
                QueryType = QueryType.SingleCode,
                DataType = DataType.Episodic,
                ResponseFormat = ResponseFormat.Influx,
                Codes = _codes
            };

            Episode episode = new Episode
            {
                CreatedBy = "testcreator",
                CreatedDate = DateTime.Today.Date.AddDays(-5),
                EndTime = DateTime.Today.Date.AddDays(-1)
            };

            _mockEpisodeService = new MockEpisodeService().ConfigureMockForGetEpisodeById(request.EpisodeId, episode);

            var testEquipmentWithoutClassifications = new Equipment
            {
                EquipmentCode = "testequipmentcode",
                EquipmentWkeId = "testequipmentwkeid",
                MaterialNumber = "testmaterialnumber",
                SerialNumber = "testserialnumber",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
            };

                    _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipmentWithoutClassifications);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCode(
                    _testEquipment.EquipmentCode, _validEquipmentModels[0]);
            
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetRows(request);

          
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public async Task Verify_GetBulkRows_Throws_NotFoundException_For_Null_Equipment()
        {
            //Arrange
            WellKnownEntityId[] testWkeidArray = new WellKnownEntityId[5];
            TimePeriod testTimePeriod = new TimePeriod(DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3));
            string[] codes = new string[5];
            DataType dtatatype = DataType.Channel;
            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(null,
                    null);
            //Act
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            _ = await apiImplementation.GetBulkRows(testWkeidArray, testTimePeriod, codes, dtatatype);
            //Assert
        }

        [TestMethod]
        public async Task Verify_GetEpisodeRows_Fetches_EpisodeRowsCollection()
        {
            string testEpisodeId = "testid";
            var testTimePeriod = new TimePeriod(DateTime.Today.AddDays(-6), DateTime.Today);

            Episode episode = new Episode
            {
                StartTime = DateTime.Today.AddDays(-5),
                EndTime = DateTime.Today.AddDays(-3),
                Name = "Episode1",
                ParentId = "ParentId1",
                Type = "Type1",
                CreatedDate = DateTime.Today.AddDays(-6),
                Id = "testid"
            };

            episode.EquipmentWkeIdList.Add(_testWkeidArray[0].Value);
            episode.EquipmentWkeIdList.Add(_testWkeidArray[1].Value);
            episode.EquipmentWkeIdList.Add(_testWkeidArray[2].Value);
            episode.EquipmentWkeIdList.Add(_testWkeidArray[3].Value);
            episode.EquipmentWkeIdList.Add(_testWkeidArray[4].Value);

            _mockEpisodeService = new MockEpisodeService().ConfigureMockForGetEpisodeById(testEpisodeId, episode);

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeIdTracked(_validEquipments);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCodeTracked(
                    _validEquipmentModels);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new MultipleChannels();
            var moqApiResponse = new ApiResponse(moqResponseEntity);

            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>()))
                .Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(ResponseFormat.V2,
                    QueryType.MultipleCodes, moqResponseProvider.Object);

            
            SortedList<string, EpicV3Hierarchy> paramList = new SortedList<string, EpicV3Hierarchy>();
            paramList.Add("5:e_code3", _testHierarchy[0]);
            paramList.Add("5:e_code6", _testHierarchy[1]);
            paramList.Add("5:e_code9", _testHierarchy[2]);
            paramList.Add("5:e_code12", _testHierarchy[3]);
            paramList.Add("5:e_code15", _testHierarchy[4]);

            _mockEpicV3HierarchyProvider = new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCodeMultiParams(paramList);

            //Act
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetEpisodeRows(testEpisodeId, testTimePeriod, DataType.Channel);

            //Assert
            _mockEpisodeService.Verify();
            _mockEquipmentProvider.Verify();
            _mockEquipmentModelProvider.Verify();
            _mockResponseProviderResolver.Verify();
        }

        [TestMethod]
        public async Task Verify_GetBulkRows_Fetches_Multiple_Channel_Response_For_Each_Equipment()
        {
            //Arrange
            TimePeriod testTimePeriod = new TimePeriod(DateTime.Today.AddDays(-5), DateTime.Today.AddDays(-3));
            string[] codes = {
                "code 1", "code 2", "code 3", "code 4", "code 5"
            };
            const DataType dttype = DataType.Channel;

            SortedList<string, EpicV3Hierarchy> paramList = new SortedList<string, EpicV3Hierarchy>();
            paramList.Add("5:e_code3", _testHierarchy[0]);
            paramList.Add("5:e_code6", _testHierarchy[1]);
            paramList.Add("5:e_code9", _testHierarchy[2]);
            paramList.Add("5:e_code12", _testHierarchy[3]);
            paramList.Add("5:e_code15", _testHierarchy[4]);

            _mockEpicV3HierarchyProvider = new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCodeMultiParams(paramList);

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeIdTracked(_validEquipments);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCodeTracked(
                    _validEquipmentModels);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new MultipleChannels();
            var moqApiResponse = new ApiResponse(moqResponseEntity);

            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>()))
                .Returns(Task.FromResult(moqApiResponse));
            
            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(ResponseFormat.V2,
                    QueryType.MultipleCodes, moqResponseProvider.Object);

            //Act
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetBulkRows(_testWkeidArray, testTimePeriod, codes, dttype);

            //Assert
            _mockEquipmentProvider.Verify();
            _mockResponseProviderResolver.Verify();
            _mockEquipmentModelProvider.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task Verify_SaveRows_With_Valid_Episode()
        {
            //Arrange
            WellKnownEntityId wkeid = new WellKnownEntityId("materialnumber01", "serialnumber01");
            DynamicInfluxRow[] influxRows = new DynamicInfluxRow[2];
            influxRows[0] = new DynamicInfluxRow();
            influxRows[1] = new DynamicInfluxRow();
            influxRows[0].Timestamp = DateTime.Now;
            DataType dataType = DataType.Episodic;
            string episodeId = "episode01";
            Episode testEpisode = new Episode();
            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "C2",
                TechnologyName = "TestHierarchy01",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> {"e_code01"},
                Status = InfluxDBStatus.Disabled
            };
            InfluxPath testPath = new InfluxPath();
            string testSuffix = "testSuffix";

            _mockEpisodeService = new MockEpisodeService().ConfigureMockForGetEpisodeById(episodeId, testEpisode);
            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(wkeid, _validEquipments[0]);
            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);
            _mockHistorianWriter =
                new MockHistorianWriter().ConfigureMockForWriteData(influxRows, testPath, testSuffix);

            //Act
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.SaveRows(wkeid, influxRows, dataType, episodeId);

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException), "Equipment mapping cannot be found")]
        public async Task Verify_GetLatestTimestampDataForChannels_WithNoMapping()
        {
            var mockWellknowEntity = new WellKnownEntityId("3213", "SBF6772134");
            var mockEquipment = new Equipment
            {
                MaterialNumber = "3213",
                SerialNumber = "SBF6772134",
                EquipmentCode = "PNG-AA",
                EquipmentWkeId = "3213:SBF6772134",
                EpicClassifications = new List<Classification>
                {
                    new Classification
                    {
                        Code="WM5",
                        ParentCode="parentcode",
                        Type="Technology"
                    },
                    new Classification
                    {
                        Code="WL-PNG",
                        ParentCode="parentcode",
                        Type="Brand"
                    },
                    new Classification
                    {
                        Code="PNG-GC",
                        ParentCode="parentcode",
                        Type="EquipmentSystem"
                    }
                }
            };
            InfluxDBMapping mockInfluxDbMapping = new InfluxDBMapping
            {
                Id="test",
                BrandName = "C2",
                TechnologyName = "TestHierarchy01",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "PNG-AA" },
                Status = InfluxDBStatus.Disabled
            };

            InfluxMappingResponse mappingInfluxDbResponse = new InfluxMappingResponse
            {
                DBName = "E_CODE3_TESTHIERARCHY01",
                DBStatus = InfluxDBStatus.Disabled,
                IsNewMeasurement = true,
                MeasurementName = "E_CODE_C2"
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(mockWellknowEntity, mockEquipment);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(mockInfluxDbMapping.Id, true, mockInfluxDbMapping);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(mockInfluxDbMapping, mappingInfluxDbResponse);
            //Act
            var apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.GetChannelLatestTimeStampWithCode(mockWellknowEntity,
                mockEquipment.EquipmentWkeId, "DischargeRate", string.Empty);
            result.Should().NotBeNull();
        }

        [TestMethod]
        public async Task Verify_GetChannelCodeTimestampDataForOkResponse()
        {
            var mockWellKnownEntityId = new WellKnownEntityId("md1", "SPF7432134");
            var mockEquipment = new Equipment
            {
                MaterialNumber = "md1",
                EquipmentCode = "SPF-743",
                EquipmentWkeId = "md1:SPF7432134",
                SerialNumber = "SPF7432134",
                SourceSystemRecordId = "5278024",
            };
            InfluxDBMapping mockInfluxDbMapping = new InfluxDBMapping
            {
               MeasurementName = "STIMULATION BLENDER OMEGA",
               DbName = "STIMULATION BLENDER OMEGA",
               EquipmentCodes = new List<string> {"SPF-743"},
               TechnologyCode = "WS61",
               BrandName = "C3",
               TechnologyName = "TestHierarchy02",
               Status = InfluxDBStatus.Disabled,
            };
            
            _mockEquipmentProvider = new MockEquipmentProviderService()
                .ConfigureMockForGetEquipmentByWkeId(mockWellKnownEntityId, mockEquipment);

            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(mockEquipment.EquipmentCode, true,
                    mockInfluxDbMapping);

            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetChannelTimestamp("www.google.com", GetInfluxResponse());
            var apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.GetChannelLatestTimeStampWithCode(mockWellKnownEntityId, "md1:SPF7432134",
                "DischargeRate", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            result.Should().NotBeNull();
            result.Should().BeOfType<List<TimestampChannelData>>();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException), EhcConstants.RecordUnavailableForThresholdValue)]

        public async Task Verify_GetChannelCodeTimestampDataForEmptyInfluxResponse()
        {
            var mockWellKnownEntityId = new WellKnownEntityId("13453", "SPF7432134");
            var mockEquipment = new Equipment
            {
                MaterialNumber = "13453",
                EquipmentCode = "SPF-744",
                EquipmentWkeId = "13453:SPF7432134",
                SerialNumber = "SPF7432134",
                SourceSystemRecordId = "5278024",
            };
            InfluxDBMapping mockInfluxDbMapping = new InfluxDBMapping
            {
                MeasurementName = "WS - 63_STIMULATION_BLENDER_PROP",
                DbName = "DXJ_WPS_BLENDING_EQUIPMENT",
                EquipmentCodes = new List<string> { "SPF-743" },
                TechnologyCode = "WS61",
                BrandName = "C3",
                TechnologyName = "TestHierarchy02",
                Status = InfluxDBStatus.Enabled,
            };

            InfluxResponse mockInfluxResponse = new InfluxResponse()
            {
                Results = new[]
                {
                    new InfluxSeries()
                    {
                        Series = new []
                        {
                            new QueryResult()
                            {
                                Columns = new List<string> {"DISCHARGE_RATE"},
                                Name = "abc",
                                Tags = new EquipmentInstanceResultSet
                                {
                                    EquipmentInstance = "md1:SPF7432134"
                                },
                                Values = new List<List<object>>
                                {
                                    new List<object>()
                                }

                            }
                        }
                    }
                }
            };
            _mockEquipmentProvider = new MockEquipmentProviderService()
                .ConfigureMockForGetEquipmentByWkeId(mockWellKnownEntityId, mockEquipment);

            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(mockEquipment.EquipmentCode, true,
                    mockInfluxDbMapping);

            _mockHistorianClient = new MockHistorianClient().ConfigureMockForGetChannelTimestamp("www.google.com", mockInfluxResponse);
            var apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.GetChannelLatestTimeStampWithCode(mockWellKnownEntityId, "13453:SPF7432134",
                "DISCHARGE_RATE", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            result.Should().NotBeNull();
            _mockInfluxDbMappingService.Verify();
            _mockEquipmentProvider.Verify();
        }

        [TestMethod]
        public async Task Verify_GetChannelDefinition_Returns_ChannelDefinitionArray()
        {
            //Arrange
            DataType dtatatype = DataType.Channel;
            
            SortedList<string, EpicV3Hierarchy> paramList = new SortedList<string, EpicV3Hierarchy>();
            paramList.Add("5:e_code3", _testHierarchy[0]);
            paramList.Add("5:e_code6", _testHierarchy[1]);
            paramList.Add("5:e_code9", _testHierarchy[2]);
            paramList.Add("5:e_code12", _testHierarchy[3]);
            paramList.Add("5:e_code15", _testHierarchy[4]);

            _mockEpicV3HierarchyProvider = new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCodeMultiParams(paramList);

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(null,
                    _validEquipments[0]);

            _mockEquipmentModelProvider =
                new MockEquipmentModelProviderService().MockEquipmentProviderServiceForGetEquipmentModelByCodeTracked(
                    _validEquipmentModels);

            QueryResult queryResult = new QueryResult
            {
                Name = "testQueryResult",
                Columns = new List<string>
                {
                    "c1", "c2", "c3"
                },
                Values = new List<List<object>>
                {
                    new List<object> {"v1", "v2", "v3"}
                }
            };

            InfluxResponse influxresponse = new InfluxResponse();
            influxresponse.Results = new InfluxSeries[1];
            influxresponse.Results[0] = new InfluxSeries();
            influxresponse.Results[0].Series = new QueryResult[2];
            influxresponse.Results[0].Series[0] = new QueryResult();
            influxresponse.Results[0].Series[0].Columns = new List<string> {"column1", "column2"};
            influxresponse.Results[0].Series[0].Name = "result0";
            influxresponse.Results[0].Series[0].Values = new List<List<object>>();
            influxresponse.Results[0].Series[0].Values.Add(new List<object> {"v1","v2"});

            _mockHistorianClient = new Mock<IHistorianClient>();
            _mockHistorianClient.Setup(o => o.PerformQuery(It.IsAny<Query>()))
                .Returns(Task.FromResult(queryResult));

            _mockHistorianClient.Setup(o => o.PerformMultiQuery(It.IsAny<Query>()))
                .Returns(Task.FromResult(influxresponse));

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetChannelDefinitions(_testWkeidArray[0], dtatatype);

            _mockEquipmentModelProvider.Verify();
            _mockHistorianClient.Verify();
            _mockChannelDefinitionService.Verify();
        }

        #region GetCalculatedRows Test
        [TestMethod]
        public async Task Verify_GetCalculatedRows_ReturnsSuccessfulResponse()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Divide,
                FillValue = "previous",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "e_code2",
                TechnologyName = "e_code3",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "e_code01" },
                Status = InfluxDBStatus.Enabled
            };

            var testEquipment = new Equipment
            {
                EquipmentCode = "e_code01",
                EquipmentWkeId = "100949474:SPF74312A0123",
                MaterialNumber = "100949474",
                SerialNumber = "SPF74312A0123",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
                {
                    new Classification
                    {
                        Code="e_code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="e_code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="e_code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipment);

            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            var result = await apiImplementation.GetCalculatedRows(request);
            query.SelectText.Should().Be("SELECT \"AirPressure.kPa\" / \"DischargeRate.m3/sec\" AS \"AirPressure_Divide_DischargeRate\" FROM \"E_CODE2_C2\" WHERE EquipmentInstance='100949474:SPF74312A0123' AND time >= 1612569600000000000 AND time <= 1615075200000000000 FILL(previous)");
            moqResponseProvider.Verify();
            result.Should().NotBeNull();

        }

        [TestMethod]
        public async Task Verify_GetCalculatedRows_BuildQuery_WithFillValueAsLinear()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Multiply,
                FillValue = "linear",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "e_code2",
                TechnologyName = "e_code3",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "e_code01" },
                Status = InfluxDBStatus.Enabled
            };

            var testEquipment = new Equipment
            {
                EquipmentCode = "e_code01",
                EquipmentWkeId = "100949474:SPF74312A0123",
                MaterialNumber = "100949474",
                SerialNumber = "SPF74312A0123",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
                {
                    new Classification
                    {
                        Code="e_code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="e_code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="e_code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipment);

            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
            query.SelectText.Should().Be("SELECT \"AirPressure.kPa\" * \"DischargeRate.m3/sec\" AS \"AirPressure_Multiply_DischargeRate\" FROM \"E_CODE2_C2\" WHERE EquipmentInstance='100949474:SPF74312A0123' AND time >= 1612569600000000000 AND time <= 1615075200000000000 FILL(linear)");

        }

        [TestMethod]
        public async Task Verify_GetCalculatedRows_BuildQuery_WithFillValueAsPrevious()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Subtract,
                FillValue = "previous",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "e_code2",
                TechnologyName = "e_code3",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "e_code01" },
                Status = InfluxDBStatus.Enabled
            };

            var testEquipment = new Equipment
            {
                EquipmentCode = "e_code01",
                EquipmentWkeId = "100949474:SPF74312A0123",
                MaterialNumber = "100949474",
                SerialNumber = "SPF74312A0123",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
                {
                    new Classification
                    {
                        Code="e_code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="e_code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="e_code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipment);

            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
            query.SelectText.Should().Be("SELECT \"AirPressure.kPa\" - \"DischargeRate.m3/sec\" AS \"AirPressure_Subtract_DischargeRate\" FROM \"E_CODE2_C2\" WHERE EquipmentInstance='100949474:SPF74312A0123' AND time >= 1612569600000000000 AND time <= 1615075200000000000 FILL(previous)");

        }

        [TestMethod]
        public async Task Verify_GetCalculatedRows_BuildQuery_WithFillValueAsNumber()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Subtract,
                FillValue = "1",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "e_code2",
                TechnologyName = "e_code3",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "e_code01" },
                Status = InfluxDBStatus.Enabled
            };

            var testEquipment = new Equipment
            {
                EquipmentCode = "e_code01",
                EquipmentWkeId = "100949474:SPF74312A0123",
                MaterialNumber = "100949474",
                SerialNumber = "SPF74312A0123",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
                {
                    new Classification
                    {
                        Code="e_code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="e_code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="e_code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipment);

            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
            query.SelectText.Should().Be("SELECT \"AirPressure.kPa\" - \"DischargeRate.m3/sec\" AS \"AirPressure_Subtract_DischargeRate\" FROM \"E_CODE2_C2\" WHERE EquipmentInstance='100949474:SPF74312A0123' AND time >= 1612569600000000000 AND time <= 1615075200000000000 FILL(1)");

        }

        [TestMethod]
        public async Task Verify_GetCalculatedRows_BuildQuery_WithFillValueSetToDefaultAsNull()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Add,
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            InfluxDBMapping testDbMapping = new InfluxDBMapping
            {
                BrandName = "e_code2",
                TechnologyName = "e_code3",
                DbName = "E_CODE3_TESTHIERARCHY01",
                MeasurementName = "E_CODE_C2",
                EquipmentCodes = new List<string> { "e_code01" },
                Status = InfluxDBStatus.Enabled
            };

            var testEquipment = new Equipment
            {
                EquipmentCode = "e_code01",
                EquipmentWkeId = "100949474:SPF74312A0123",
                MaterialNumber = "100949474",
                SerialNumber = "SPF74312A0123",
                SourceSystemRecordId = "testsourcesystemrecordid",
                EpicClassifications = new List<Classification>()
                {
                    new Classification
                    {
                        Code="e_code01",
                        ParentCode = "ParentCode",
                        Type = "EquipmentSystem"
                    },
                    new Classification{
                        Code="e_code2",
                        ParentCode = "ParentCode",
                        Type = "Brand"
                    },
                    new Classification{
                        Code="e_code3",
                        ParentCode = "ParentCode",
                        Type = "Technology"
                    }}
            };

            _mockEquipmentProvider =
                new MockEquipmentProviderService().ConfigureMockForGetEquipmentByWkeId(request.WKEid,
                    testEquipment);

            _mockEpicV3HierarchyProvider =
                new MockEpicV3HierarchyProvider().ConfigureMockForGetEpicHierarchyInfoFromCode("5:e_code3", _testHierarchy[0]);
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForGetInfluxDbMapping(_validEquipments[0].EquipmentCode, true, testDbMapping);
            InfluxMappingResponse mappingResponse = new InfluxMappingResponse();
            _mockInfluxDbMappingService =
                new MockInfluxDbMappingService().ConfigureMockForCreateUpdateDbMapping(testDbMapping, mappingResponse);

            var moqResponseProvider = new Mock<ResponseProvider>();
            var moqResponseEntity = new Mock<MultipleChannels>();
            var moqApiResponse = new ApiResponse(moqResponseEntity.Object);

            Query query = null;
            moqResponseProvider.Setup(o => o.GetResponse(It.IsAny<Query>(), It.IsAny<QueryContext>())).Callback<Query, QueryContext>((q, qContext) => query = q).Returns(Task.FromResult(moqApiResponse));

            _mockResponseProviderResolver =
                new MockResponseProviderResolver().ConfigureMockForGetResponseProvider(request.ResponseFormat,
                    request.QueryType, moqResponseProvider.Object);

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");

            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
            query.SelectText.Should().Be("SELECT \"AirPressure.kPa\" + \"DischargeRate.m3/sec\" AS \"AirPressure_Add_DischargeRate\" FROM \"E_CODE2_C2\" WHERE EquipmentInstance='100949474:SPF74312A0123' AND time >= 1612569600000000000 AND time <= 1615075200000000000 FILL(null)");

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException), EhcConstants.ChannelDefinitionNotFoundForCode + "dummy")]
        public async Task Verify_GetCalculatedRows_ReturnsNotChannelCodeNotFound()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Divide,
                FillValue = "previous",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };
            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForValidateChannelCodeThrowsException();
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);

        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), EhcConstants.InvalidFillValue)]
        public async Task Verify_GetCalculatedRows_ReturnsBadRequestForInvalidFillValue()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Divide,
                FillValue = "hjhjhjh",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa","DischargeRate.m3/sec");
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException),EhcConstants.InvalidFillValue)]
        public async Task Verify_GetCalculatedRows_ReturnsBadRequestForFillValueNotANumber()
        {
            RowsRequest request = new RowsRequest
            {

                DataType = DataType.Channel,
                QueryType = QueryType.MultipleCodes,
                ResponseFormat = ResponseFormat.V2,
                Codes = new[] { "AirPressure", "DischargeRate" },
                MathFunction = MathFunctions.Multiply,
                FillValue = "1.8",
                TimePeriod = new TimePeriod(new DateTime(2021, 02, 06), new DateTime(2021, 03, 07))
            };

            _mockChannelDefinitionService =
                new MockChannelDefinitionService().ConfigureMockForGetFieldNameSequence("AirPressure.kPa", "DischargeRate.m3/sec");
            ApiImplementation apiImplementation = GetApiImplementationInstance();
            await apiImplementation.GetCalculatedRows(request);
        }


        #endregion

        private ApiImplementation GetApiImplementationInstance()
        {
            _mockEquipmentProvider ??= new Mock<IEquipmentProvider>();

            _mockEquipmentModelProvider ??= new Mock<IEquipmentModelProvider>();

            _mockEpicV3HierarchyProvider ??= new Mock<IEpicV3HierarchyProvider>();

            _mockResponseProviderResolver ??= new Mock<IResponseProviderResolver>();

            _mockHistorianClient ??= new Mock<IHistorianClient>();

            _mockChannelDefinitionService ??= new Mock<IChannelDefinitionService>();

            _mockHyperLinksProvider ??= new Mock<IHyperLinksProvider>();

            _mockHistorianWriter ??= new Mock<IHistorianWriter>();

            _mockEpisodeService ??= new Mock<IEpisodeService>();

            _mockChannelDefinitionService ??= new Mock<IChannelDefinitionService>();

            _mockInfluxDbMappingService ??= new Mock<IInfluxDBMappingService>();

            _apiConfig ??= ConfigDetails();

            GetInfluxResponse();

            _urlBuilder ??= new Mock<IUrlBuilder>();

            GetHistorianClient();

            _timeStampParser ??= new Mock<ITimestampParser>();

            return new ApiImplementation(_mockEquipmentProvider.Object, _mockEpicV3HierarchyProvider.Object,
                _mockResponseProviderResolver.Object,
                _mockHistorianClient.Object,
                _mockHyperLinksProvider.Object, _mockHistorianWriter.Object,
                _mockEpisodeService.Object, _mockChannelDefinitionService.Object, _apiConfig, _mockInfluxDbMappingService.Object,_urlBuilder.Object,_timeStampParser.Object);
        }

        [TestCleanup]
        public void ResetMockObjectState()
        {
            _mockEquipmentProvider = null;
            _mockEquipmentModelProvider = null;
            _mockEpicV3HierarchyProvider = null;
            _mockResponseProviderResolver = null;
            _mockHistorianClient = null;
            _mockHyperLinksProvider = null;
            _mockHistorianWriter = null;
            _mockEpisodeService = null;
            _mockChannelDefinitionService = null;
            _mockInfluxDbMappingService = null;
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
