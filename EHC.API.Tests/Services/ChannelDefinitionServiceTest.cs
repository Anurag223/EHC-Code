using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Data;
using Tlm.Sdk.Core.Models.Querying;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Services;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace EHC.API.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class ChannelDefinitionServiceTest
    {
        private MockRepository _mockProvider;
        Mock<IRepositoryHandler<ChannelDefinition>> _mockRepositoryHandler;
        private IChannelDefinitionService _service;
        private Mock<IEpicV3HierarchyProvider> _epicV3Hierarchy;
        private const string ChannelCode = "DischargeRate";
        private const string InvalidEquipmentCode = "SPF-333";

        private EhcApiConfig GetConfigDetails()
        {
            return new EhcApiConfig
            {
                ServiceCacheTimeDuration = 24

            };
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockRepositoryHandler = _mockProvider.Create<IRepositoryHandler<ChannelDefinition>>();
            _epicV3Hierarchy = _mockProvider.Create<IEpicV3HierarchyProvider>();
            _service = new ChannelDefinitionService(_mockRepositoryHandler.Object, new MemoryCacheFake(),
                GetConfigDetails(), _epicV3Hierarchy.Object);
        }

        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenIdNotNull_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testcode",
                Id = "testid"
            };
            var result = _service.CreateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be("ChannelDefinition.Id should be null.");

        }

        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenUomIsNull_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testcode",
                Dimension = "testdimension",
                Uom = null

            };
            var result = _service.CreateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be("Empty ChannelDefinition.Uom");

        }

        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenCodeIsNull_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = null,
                Dimension = "testdimension",
                Uom = "testuom"

            };
            var result = _service.CreateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be("Empty ChannelDefinition.Code");


        }

        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenDimensionIsNull_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = null,
                Uom = "testUom"

            };
            var result = _service.CreateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be("Empty ChannelDefinition.Dimension");

        }

        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenChannelDefinitionAlreadyExists_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                {
                    cd
                }));

            var result = _service.CreateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<BadRequestException>();
            result.Exception?.InnerException?.Message.Should().Be("ChannelDefinition already exists: testCode");

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException),
            "Equipment code " + InvalidEquipmentCode + " not found in Epic V3 Hierarchy")]
        public async Task Verify_CreateChannelDefinition__ThrowsException_WhenEquipmentCodeNotFound()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom",
                EquipmentCodes = new List<string>(){ InvalidEquipmentCode }
            };
            string code = InvalidEquipmentCode;
            _epicV3Hierarchy.Setup(o => o.GetEpicHierarchyInfoFromCode(It.IsAny<string>())).Throws(
                new NotFoundException($"Equipment code {code.Substring(2)} not found in Epic V3 Hierarchy")
                    { ErrorCode = ErrorCodes.EquipmentCodeNotFound });
            await _service.CreateChannelDefinition(cd);
        }


        [TestMethod]
        public void Verify_CreateChannelDefinition_WhenChannelDefinitionDoesNotExists_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()));

            var result = _service.CreateChannelDefinition(cd);

            _mockRepositoryHandler.Verify(o => o.UpdateAsync(It.IsAny<ChannelDefinition>()), Times.Once);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<Exception>();
            result.Exception?.InnerException?.Message.Should().Be("Got empty id after creating an episode.'");

        }

        [TestMethod]
        public void Verify_DeleteChannelDefinition_WhenChannelDefinitionExists()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                {
                    cd
                }));

            _mockRepositoryHandler.Setup(o => o.DeleteManyAsync(It.IsAny<DeleteSpec>(), null))
                .Returns(Task.FromResult<long?>(0));


            var result = _service.DeleteChannelDefinition("testCode");

            result.Status.Should().Be(TaskStatus.Faulted);

        }

        [TestMethod]
        public void Verify_DeleteChannelDefinition_WhenChannelDefinitionNotFound_ThrowsException()
        {
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()));

            _mockRepositoryHandler.Setup(o => o.DeleteManyAsync(It.IsAny<DeleteSpec>(), null))
                .Returns(Task.FromResult<long?>(0));


            var result = _service.DeleteChannelDefinition("testCode");
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<NotFoundException>();
            result.Exception?.InnerException?.Message.Should().Be("ChannelDefinition not found: testCode");

        }

        [TestMethod]
        public void Verify_UpdateChannelDefinition_WhenChannelDefinitionNotFound_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()));

            var result = _service.UpdateChannelDefinition(cd);
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<NotFoundException>();
            result.Exception?.InnerException?.Message.Should().Be("ChannelDefinition not found: testCode");

        }

        [TestMethod]
        public void Verify_UpdateChannelDefinition_WhenChannelDefinitionExists()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                {
                    cd
                }));

            _service.UpdateChannelDefinition(cd);

            _mockRepositoryHandler.Verify(o => o.UpdateAsync(cd), Times.Once);

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException),
            "Equipment code " + InvalidEquipmentCode + " not found in Epic V3 Hierarchy")]
        public async Task Verify_UpdateChannelDefinition__ThrowsException_WhenEquipmentCodeNotFound()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom",
                EquipmentCodes = new List<string>() { InvalidEquipmentCode }
            };
            string code = InvalidEquipmentCode;
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>() { cd }));
            _epicV3Hierarchy.Setup(o => o.GetEpicHierarchyInfoFromCode(It.IsAny<string>())).Throws(
                new NotFoundException($"Equipment code {code.Substring(2)} not found in Epic V3 Hierarchy")
                    { ErrorCode = ErrorCodes.EquipmentCodeNotFound });
            await _service.UpdateChannelDefinition(cd);
        }

        [TestMethod]
        public void Verify_GetFieldName_WhenChannelDefinitionExists()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testUom"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                {
                    cd
                }));

            var result = _service.GetFieldName("testCode");
            result.Result.Should().Be("testCode.testUom");
        }

        [TestMethod]
        public void Verify_GetFieldName_WhenChannelDefinitionNotFound()
        {
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                ));

            var result = _service.GetFieldName("testCode");
            result.Result.Should().Be("testCode");

        }

        [TestMethod]
        public void Verify_GetFieldName_ForChannelDefinitionWhenUomNotFound_ThrowsException()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                    { cd }));

            var result = _service.GetFieldName("testCode");
            result.Exception.Should().BeOfType<AggregateException>();
            result.Exception?.InnerException.Should().BeOfType<ArgumentException>();
            result.Exception?.InnerException?.Message.Should().Be("Empty UOM in channel definition for code: testCode");

        }

        [TestMethod]
        public void Verify_GetFieldName_ForChannelDefinitionWhenUomIsUnitless()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "unitless"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                    { cd }));

            var result = _service.GetFieldName("testCode");
            result.Result.Should().Be("testCode");

        }

        [TestMethod]
        public void Verify_GetChannelDescription_Successful()
        {
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "testCode",
                Dimension = "testdimension",
                Uom = "testDimension"
            };

            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                    { cd }));

            var result = _service.GetChannelDescription("testCode.testDimension");
            result.Result.Should().BeOfType<ChannelDefinitionClean>();

        }

        [TestMethod]
        public void Verify_GetChannelDescription_WhenChannelDefinitionNotFound()
        {
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()
                ));

            var result = _service.GetChannelDescription("test.Dimension");

            result.Result.Should().BeOfType<ChannelDefinitionClean>();
            result.Result.Code.Should().Be("test");
            result.Result.Uom.Should().Be("Dimension");

        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException), "Channel definition not found for code:" + ChannelCode)]
        public async Task Test_ValidateChannelCode_ThrowsExceptionWhenChannelCodeNotFound()
        {
            List<string> channelCodes = new List<string>()
            {
                "AirPressure",
                "test"
            };
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>()));
            await _service.ValidateChannelCode(channelCodes);
        }

        [TestMethod]
        public async Task Test_ValidateChannelCode_Successful()
        {
            List<string> channelCodes = new List<string>()
            {
                "AirPressure",
                "DischargeRate"
            };
            ChannelDefinition cd = new ChannelDefinition()
            {
                Code = "AirPressure",
                Id = "12345"
            };
            _mockRepositoryHandler.Setup(o => o.GetAsync(It.IsAny<Expression<Func<ChannelDefinition, bool>>>()))
                .Returns(Task.FromResult(new List<ChannelDefinition>(){cd}));
            await _service.ValidateChannelCode(channelCodes);
            _mockRepositoryHandler.Verify();
        }

        [TestMethod]
        [ExpectedException(typeof(NotFoundException),
            "Equipment code " + InvalidEquipmentCode + " not found in Epic V3 Hierarchy")]
        public async Task Test_ValidateEquipmentCode_ThrowsExceptionWhenEquipmentCodeNotFound()
        {
            string code = InvalidEquipmentCode;
            _epicV3Hierarchy.Setup(o => o.GetEpicHierarchyInfoFromCode(It.IsAny<string>())).Throws(
                new NotFoundException($"Equipment code {code.Substring(2)} not found in Epic V3 Hierarchy")
                    { ErrorCode = ErrorCodes.EquipmentCodeNotFound });
            await _service.ValidateEquipmentCode(code);
        }

        [TestMethod]
        public async Task Test_ValidateEquipmentCode_Successful()
        {
            string code = InvalidEquipmentCode;
            _epicV3Hierarchy.Setup(o => o.GetEpicHierarchyInfoFromCode(It.IsAny<string>())).Returns(Task.FromResult(new EpicV3Hierarchy()));
            await _service.ValidateEquipmentCode(code);
            _epicV3Hierarchy.Verify();

        }

        [TestMethod]
        public async Task Test_UpdateEquipmentCodeOnChannelDefinition_WhenEquipmentCodeNotPresentInDb()
        {
            const string code = "SPF-743";
            var eqCode = new List<string> { code };
            var inputChannelCodes = new List<string>() { "AirPressure", "DischargeRate" };
            var querySpec1 = QuerySpec.ByValues("equipmentcodes", eqCode);
            var cr1 =
                new CollectionResult<ChannelDefinition>(new List<ChannelDefinition>());
            _mockRepositoryHandler.Setup(o => o.QueryManyAsync(querySpec1))
                .Returns(Task.FromResult(cr1));

            var querySpec2 = QuerySpec.ByValues("code", inputChannelCodes);
            var cd1 = new ChannelDefinition()
            {
                Code = "AirPressure",
                Id = "AnyId1"
            };
            var cd2 = new ChannelDefinition()
            {
                Code = "DischargeRate",
                Id = "AnyId1"
            };
            var channelDef = new List<ChannelDefinition>() { cd1, cd2 };
            var cr2 =
                new CollectionResult<ChannelDefinition>(channelDef);
            _mockRepositoryHandler.Setup(o => o.QueryManyAsync(querySpec2))
                .Returns(Task.FromResult(cr2));
            IReadOnlyCollection<ChannelDefinition> chDef = channelDef;

            _mockRepositoryHandler.Setup(o => o.UpdateManyAsync(chDef))
                .Returns(Task.FromResult(new UpdateResult<ChannelDefinition>(chDef, chDef)));

            await _service.UpdateEquipmentCodeOnChannelDefinition(code,inputChannelCodes);
            _mockRepositoryHandler.Verify(m => m.UpdateManyAsync(chDef));
            _mockRepositoryHandler.VerifyAll();
          
        }

        [TestMethod]
        public async Task Test_UpdateEquipmentCodeOnChannelDefinition_WhenEquipmentCodePresentInDbAndSomeEqAddition()
        {
            const string code = "SPF-743";
            var inputChannelCodes = new List<string>() { "DischargeRate" };
            var cd1 = new ChannelDefinition()
            {
                Code = "AirPressure",
                Id = "AnyId1",
                EquipmentCodes = new List<string>(){code}
            };
            var cr1 =
                new CollectionResult<ChannelDefinition>(new List<ChannelDefinition>(){cd1});

            var cd2 = new ChannelDefinition()
            {
                Code = "DischargeRate",
                Id = "AnyId2"
            };
            var channelDef1 = new List<ChannelDefinition>() { cd1 };
            var channelDef2 = new List<ChannelDefinition>() { cd2 };
            var cr2 =
                new CollectionResult<ChannelDefinition>(channelDef2);

            _mockRepositoryHandler.SetupSequence(m => m.QueryManyAsync(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(cr1))
                .Returns(Task.FromResult(cr2));
            IReadOnlyCollection<ChannelDefinition> chDef1 = channelDef1;
            IReadOnlyCollection<ChannelDefinition> chDef2 = channelDef2;

            _mockRepositoryHandler.SetupSequence(m => m.UpdateManyAsync(It.IsAny<IReadOnlyCollection<ChannelDefinition>>()))
                .Returns(Task.FromResult(new UpdateResult<ChannelDefinition>(chDef1, chDef1)))
                .Returns(Task.FromResult(new UpdateResult<ChannelDefinition>(chDef2, chDef2)));

            await _service.UpdateEquipmentCodeOnChannelDefinition(code, inputChannelCodes);

            _mockRepositoryHandler.Verify(m=>m.UpdateManyAsync(chDef1));
           _mockRepositoryHandler.Verify(m => m.UpdateManyAsync(chDef2));
           _mockRepositoryHandler.VerifyAll();

        }

        // Fake class for Memory Cache added
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

            public void Set(object o, ChannelDefinition cd, TimeSpan t)
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
}
