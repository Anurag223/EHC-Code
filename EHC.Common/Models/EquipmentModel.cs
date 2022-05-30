using System;
using System.Collections.Generic;

namespace TLM.EHC.Common.Models
{
    public class EquipmentModel
    {
        public string EquipmentCode { get; set; }
        public string Description { get; set; }
        public string MaterialNumber { get; set; }

        public string TechnologyCode { get; set; }
        public string BrandCode { get; set; }

        public string TechnologyName { get; set; }
        public string BrandName { get; set; }

        public List<EquipmentModelChannel> Channels { get; set; }
    }


    public class EquipmentModelChannel
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Dimension { get; set; }
        public string Uom { get; set; }
        public string LegalClassification { get; set; }
    }
}
