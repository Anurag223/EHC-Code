using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.Admin;

namespace TLM.EHC.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class DbMapResponse
    {
        public string EquipmentCode { get; set; }
        public string DbName { get; set; }
        public string MeasurementName { get; set; }
        public InfluxDBStatus Status { get; set; }
        public DBMapConflictStatus ConflictStatus { get; set; }
    }
}
