using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.Common.Models;

namespace EHC.API.Tests
{
    [UnitTestCategory]
    [TestClass]
    public class InfluxPathTests
    {
        
        [TestMethod]
        public void DXJ()
        {
            EquipmentModel equipmentModel = new EquipmentModel();

            equipmentModel.EquipmentCode = "SBF-624";

            equipmentModel.TechnologyCode = "DXJ";
            equipmentModel.TechnologyName = "WPS BLENDING EQUIPMENT";

            equipmentModel.BrandCode = "WS-63";
            equipmentModel.BrandName = "STIMULATION BLENDER PROP";

            var path = InfluxPath.GetFromEquipmentModel(equipmentModel);

            
            path.Technology.Should().Be("DXJ_WPS_BLENDING_EQUIPMENT");
            path.Brand.Should().Be("WS-63_STIMULATION_BLENDER_PROP");
        }



        [TestMethod]
        public void WU2()
        {
            EquipmentModel equipmentModel = new EquipmentModel();

            equipmentModel.EquipmentCode = "MCMU-FA";

            equipmentModel.TechnologyCode = "WU2";
            equipmentModel.TechnologyName = "MECHANICAL/CORING";

            equipmentModel.BrandCode = "WL-085";
            equipmentModel.BrandName = "MSCT";

            var path = InfluxPath.GetFromEquipmentModel(equipmentModel);

            path.Technology.Should().Be("WU2_MECHANICAL_CORING");
            path.Brand.Should().Be("WL-085_MSCT");
        }


    }
}
