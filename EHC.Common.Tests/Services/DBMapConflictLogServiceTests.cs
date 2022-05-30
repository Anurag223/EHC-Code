using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NSubstitute;
using TLM.EHC.Admin;
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
    public class DBMapConflictLogServiceTests
    {
        private MockRepository _mockProvider;
        Mock<IRepositoryHandler<EpicDBMapConflictLog>> _mockRepositoryHandler;
        private DBMapConflictLogService _service;


        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockRepositoryHandler = _mockProvider.Create<IRepositoryHandler<EpicDBMapConflictLog>>();
            _service = new DBMapConflictLogService(_mockRepositoryHandler.Object, new MemoryCacheFake());
        }

        [TestMethod]
        public void Test_GetConflictStatus_ByEquipmentCode()
        {
            EpicDBMapConflictLog DbConflictMockData = new EpicDBMapConflictLog()
            {
                EpicEquipmentCode = "SBF-624",
                DBMapEquipmentCode = "SBF-624",
                EpicBrandCode = "B1",
                DBMapBrandCode = "B2",
                EpicTechnologyCode = "T1",
                DBMapTechnologyCode = "T2",
                CreatedDate = DateTime.Parse(DateTime.UtcNow.ToString())
            };

            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<EpicDBMapConflictLog, bool>>>()))
                .Returns(Task.FromResult(new List<EpicDBMapConflictLog>() { DbConflictMockData }));
            var result = _service.GetConflictStatusByEquipmentCode(DbConflictMockData.DBMapEquipmentCode, false);
            result.Result.Should().BeOfType<DBMapConflictStatus>();
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task Test_GetConflictStatus_ForEmptyEquipmentCode()
        {
            _mockRepositoryHandler.Setup(x => x.GetAsync(It.IsAny<Expression<Func<EpicDBMapConflictLog, bool>>>()))
                .Returns(Task.FromResult(new List<EpicDBMapConflictLog>()
                {

                }));
            await _service.GetConflictStatusByEquipmentCode(null, false);
        }

        [TestMethod]
        public async Task Test_GetAllEpicDBMapConflictLogByCriteria_Successful()
        {
            List<EpicDBMapConflictLog> list = new List<EpicDBMapConflictLog>()
            {
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T1",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow.AddDays(-1)
                },
                 new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T2",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
                },
               new EpicDBMapConflictLog(){
                   DBMapEquipmentCode="Eq2",
                    DBMapBrandCode="BC3",
                    EpicBrandCode ="BC4",
                    DBMapTechnologyCode="T3",
                    EpicTechnologyName="T4",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
               },
            };
            CollectionResult<EpicDBMapConflictLog> coll = new CollectionResult<EpicDBMapConflictLog>(list);
            _mockRepositoryHandler.Setup(x => x.QueryManyAsync(It.IsAny<QuerySpec>()))
                .Returns(Task.FromResult(coll));

            var result = await _service.GetAllEpicDBMapConflictLogByCriteria(QuerySpec.ForEverything);
            Assert.That.Should().NotBeNull();
            Assert.AreEqual(result.Collection.Count, list.Count);
        }

        [TestMethod]
        public async Task Test_GetAllEpicDBMapConflictLogs_Successful()
        {
            List<EpicDBMapConflictLog> list = new List<EpicDBMapConflictLog>()
            {
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T1",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow.AddDays(-1)
                },
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq1",
                    DBMapBrandCode="BC1",
                    EpicBrandCode ="BC2",
                    DBMapTechnologyCode="T1",
                    EpicTechnologyName="T2",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
                },
                new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq2",
                    DBMapBrandCode="BC3",
                    EpicBrandCode ="BC4",
                    DBMapTechnologyCode="T3",
                    EpicTechnologyName="T4",
                    ConflictStatus=DBMapConflictStatus.OutOfSync,
                    CreatedDate= DateTime.UtcNow
                }
                ,  new EpicDBMapConflictLog(){
                    DBMapEquipmentCode="Eq3",
                    DBMapBrandCode="BC5",
                    EpicBrandCode ="BC6",
                    DBMapTechnologyCode="T5",
                    EpicTechnologyName="T6",
                    ConflictStatus=DBMapConflictStatus.InSync,
                    CreatedDate= DateTime.UtcNow
                },
            };

            _mockRepositoryHandler.Setup(o =>
                    o.GetAsync(It.IsAny<Expression<Func<EpicDBMapConflictLog, bool>>>()))
                .Returns(Task.FromResult(list.Where(x => x.ConflictStatus == DBMapConflictStatus.OutOfSync).ToList()));

            var result = await _service.GetAllEpicDBMapConflictLogs();
            Assert.That.Should().NotBeNull();
            Assert.AreEqual(3, result.Count);
        }

    }

}

