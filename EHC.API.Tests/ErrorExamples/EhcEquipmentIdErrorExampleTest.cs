using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Common.ErrorExamples;

namespace EHC.API.Tests.ErrorExamplesTest
{
    [UnitTestCategory]
    [TestClass]
   public class EhcEquipmentIdErrorExampleTest
    {
        [TestMethod]
        public void Verify_InvalidEquipmentWkeid_ErrorExample()
        {
            var error = EhcEquipmentIdErrorExample.InvalidEquipmentWkeid("code", "xyz");
            Assert.AreEqual(error.Code, "INVALIDEQUIPMENTWKEID");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "Invalid EquipmentWkeId xyz,':' symbol not found");

        }

        [TestMethod]
        public void Verify_EquipmentNotFound_ErrorExample()
        {
            var error = EhcEquipmentIdErrorExample.EquipmentNotFound("code", "xyz");
            Assert.AreEqual(error.Code, "EQUIPMENTNOTFOUND");
            Assert.AreEqual(error.Status, "404");
            Assert.AreEqual(error.StatusCode, 404);
            Assert.AreEqual(error.Detail, "Equipment not found: xyz");

        }
    }
}
