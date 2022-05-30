using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace TLM.EHC.API.Stubs
{
    public class StubEquipmentModelProvider : IEquipmentModelProvider
    {
        public async Task<EquipmentModel> GetEquipmentModelByCode(string equipmentCode)
        {
            if (string.IsNullOrWhiteSpace(equipmentCode))
            {
                throw new ArgumentException("Empty equipmentCode.");
            }

            if (equipmentCode.StartsWith("MCMU-") || equipmentCode.StartsWith("MRCM-"))
            {
                return new EquipmentModel()
                {
                    EquipmentCode = equipmentCode,
                    TechnologyCode = "WU2",
                    TechnologyName = "MECHANICAL/CORING",
                    BrandCode = "WL-085",
                    BrandName = "MSCT",
                    Channels = new List<EquipmentModelChannel>()
                };
            }


            switch (equipmentCode)
            {
                case "SBF-624": 
                    return new EquipmentModel()
                    {
                        EquipmentCode =  equipmentCode,
                        TechnologyCode = "DXJ",
                        TechnologyName = "WPS BLENDING EQUIPMENT",
                        BrandCode = "WS-63",
                        BrandName = "STIMULATION BLENDER PROP",
                        Channels = new List<EquipmentModelChannel>()
                    };
            }


            throw new ServerErrorException("Hard-coded stub for Equipment Model provider has no Technology+Brand defined for EquipmentCode: " + equipmentCode);
        }
    }
}
