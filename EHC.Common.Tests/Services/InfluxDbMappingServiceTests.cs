using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.Admin;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using Tlm.Sdk.Testing.Unit;

namespace EHC.Common.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class InfluxDbMappingServiceTests
    {
        private MockRepository _mockProvider;
        Mock<IRepositoryHandler<InfluxDBMapping>> _mockRepositoryHandler;
        private Mock<IEpicV3HierarchyProvider> _mockEpicV3HierarchyProvider;
        private IInfluxDBMappingService _service;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockRepositoryHandler = _mockProvider.Create<IRepositoryHandler<InfluxDBMapping>>();
            _mockEpicV3HierarchyProvider = _mockProvider.Create<IEpicV3HierarchyProvider>();
            _service = new InfluxDBMappingService(_mockRepositoryHandler.Object, new MemoryCacheFake(),GetCacheDurationDetails());
            
        }

        private EhcApiConfig GetCacheDurationDetails()
        {
            return new EhcApiConfig
            {
                ServiceCacheTimeDuration = 24
            };
        }

        [TestMethod]

        public void Test_GetInfluxDBName()
        {
            InfluxDBMapping influxDbMockData = new InfluxDBMapping()
            {
                BrandName = "testBrand",
                DbName = "TestDb",
                EquipmentCodes = new List<string>() { "SPF-743", "SPF-343" },
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Pumping_Equipment",
                MeasurementName = "WS-63_Blending_Equipment"
            };
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    influxDbMockData
                }));

            var result = _service.GetInfluxDBName("SPF-743");
            result.Result.Should().BeOfType<InfluxDBMapping>();
            Assert.AreEqual("WPS_Pumping_Equipment", result.Result.TechnologyName);
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException))]
        public async Task Test_InfluxDBName_WithNoDBMap_ThrowsException()
        {           
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {                 
                }));
            var result = await _service.GetInfluxDBName("SPF-743");
        }

        [TestMethod]
        public void Test_GetInfluxDBMapping()
        {
            InfluxDBMapping influxDbMockData = new InfluxDBMapping()
            {
                BrandName = "testBrand",
                DbName = "TestDb",
                EquipmentCodes = new List<string>() { "SPF-743", "SPF-343" },
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Pumping_Equipment",
                MeasurementName = "WS-63_Blending_Equipment"
            };

            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>() { influxDbMockData }));
            var result = _service.GetInfluxDBMapping(influxDbMockData.EquipmentCodes[0], false);
            result.Result.Should().BeOfType<InfluxDBMapping>();
        }

        [TestMethod]
        public async Task Test_GetInfluxDBMapping_ForEmptyEquipmentCodes()
        {
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {

                }));
            var result = await _service.GetInfluxDBMapping(null, false);
            Assert.AreEqual(null, result);
        }

        [TestMethod]
        public async Task Test_CreateUpdateDBMapping()
        {
            InfluxDBMapping influxDbMockData = new InfluxDBMapping()
            {
                BrandName = "testBrand",
                DbName = "TestDb",
                EquipmentCodes = new List<string>() { "SPF-743", "SPF-343" },
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Pumping_Equipment",
                MeasurementName = "WS-63_Blending_Equipment"
            };

            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    influxDbMockData
                }));
            var result = await _service.CreateUpdateDBMapping(influxDbMockData);
            _mockRepositoryHandler.Setup(m => m.UpdateAsync(It.IsAny<InfluxDBMapping>()));
            Assert.That.Should().NotBeNull();
            result.Should().BeOfType<InfluxMappingResponse>();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Empty DB name")]
        public async Task Test_Validate_For_NoDbMapping()
        {
            InfluxDBMapping influxDbMockData = new InfluxDBMapping()
            {
                BrandName = "testBrand",
                EquipmentCodes = new List<string>() { "SPF-743", "SPF-343" },
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Pumping_Equipment",
                MeasurementName = "WS-63_Blending_Equipment"
            };

            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    influxDbMockData
                }));
            var result = await _service.CreateUpdateDBMapping(influxDbMockData);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "No equipment codes")]
        public async Task Test_ValidateInfluxDBMapping_ForEmptyEquipmentCodes()
        {
            InfluxDBMapping mockInfluxDbData = new InfluxDBMapping()
            {
                DbName = "testdb",
                BrandName = "WS-63_Pump_Continuous",
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Surface_Equipment"
            };
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    mockInfluxDbData
                }));
            var result = await _service.CreateUpdateDBMapping(mockInfluxDbData);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Empty Measurement Name")]
        public async Task Test_ValidateInfluxDBMapping_ForEmptyMeasurement()
        {
            InfluxDBMapping mockInfluxDbData = new InfluxDBMapping()
            {
                DbName = "testdb",
                BrandName = "WS-63_Pump_Continuous",
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Surface_Equipment",
                EquipmentCodes = new List<string>() { "SPF-743,SPF-343" }
            };
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    mockInfluxDbData
                }));
            var result = await _service.CreateUpdateDBMapping(mockInfluxDbData);
        }
        [TestMethod]
        public async Task Test_GetInfluxBrandName()
        {
            InfluxDBMapping mockInfluxDbData = new InfluxDBMapping()
            {
                DbName = "testdb",
                BrandName = "WS-63_Pump_Continuous",
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Surface_Equipment",
                EquipmentCodes = new List<string>() { "SPF-743" },
                MeasurementName = "testMeasurement"
            };
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<InfluxDBMapping, bool>>>()))
                .Returns(Task.FromResult(new List<InfluxDBMapping>()
                {
                    mockInfluxDbData
                }));
            var result = await _service.CreateUpdateDBMapping(mockInfluxDbData);
            result.Should().BeOfType<InfluxMappingResponse>();
            Assert.AreEqual("WS-63 Pump Continuous", mockInfluxDbData.BrandName);
        }

        [TestMethod]
        public async Task Test_GetAllInfluxDBMappingData_Successful()
        {
            List<InfluxDBMapping> maps = new List<InfluxDBMapping>();
            InfluxDBMapping influxDbMockData1 = new InfluxDBMapping()
            {
                BrandName = "testBrand",
                DbName = "TestDb",
                EquipmentCodes = new List<string>() { "SPF-743", "SPF-343" },
                RetentionPolicy = "lifetime",
                TechnologyName = "WPS_Pumping_Equipment",
                MeasurementName = "WS-63_Blending_Equipment"
            };
            maps.Add(influxDbMockData1);
            InfluxDBMapping influxDbMockData2 = new InfluxDBMapping()
            {
                BrandName = "testBrand2",
                DbName = "TestDb2",
                EquipmentCodes = new List<string>() { "SBF-677" },
                RetentionPolicy = "lifetime",
                TechnologyName = "techname",
                MeasurementName = "mname"
            };
            maps.Add(influxDbMockData2);
            CollectionResult<InfluxDBMapping> coll = new CollectionResult<InfluxDBMapping>(maps);

            _mockRepositoryHandler.Setup(x => x.QueryManyAsync(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(coll));

            var result = await _service.GetAllInfluxDBMappingData(QuerySpec.ForEverything);
            Assert.That.Should().NotBeNull();
            Assert.AreEqual(result.Collection.Count, maps.Count);
        }

    }


    public class MemoryCacheFake : IMemoryCache
    {
        public ICacheEntry CreateEntry(object key)
        {
            return new CacheEntryFake { Key = key };
        }

        public void Dispose()
        {

        }

        public void Remove(object key)
        {

        }

        public void Set(object o, InfluxDBMapping influxDb, TimeSpan t)
        {

        }

        public bool TryGetValue(object key, out object value)
        {
            value = null;
            return false;
        }
    }

    public class CacheEntryFake : ICacheEntry
    {
        public object Key { get; set; }

        public object Value { get; set; }
        public DateTimeOffset? AbsoluteExpiration { get; set; }
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }
        public TimeSpan? SlidingExpiration { get; set; }

        public IList<IChangeToken> ExpirationTokens { get; set; }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; set; }

        public CacheItemPriority Priority { get; set; }
        public long? Size { get; set; }

        public void Dispose()
        {

        }
    }
}

