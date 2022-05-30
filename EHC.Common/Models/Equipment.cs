using System.Collections.Generic;
using TLM.EHC.Common.Clients.EquipmentApi;

namespace TLM.EHC.Common.Models
{
    public class Equipment
    {
        public string EquipmentWkeId { get; set; }

        public string MaterialNumber { get; set; } // will be maximo assetnumber 
        public string SerialNumber { get; set; }
        public string EquipmentCode { get; set; }

        // temp field
        public string SourceSystemRecordId { get; set; }   // as of April 8, 2020 is maximo assetnumber 
        public List<Classification> EpicClassifications { get; set; }

    }


}
