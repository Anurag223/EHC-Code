using TLM.EHC.Admin;

namespace TLM.EHC.Common.Models
{
    public class InfluxMappingResponse
    {         
        public bool IsNewMeasurement { get; set; }
        public string MeasurementName { get; set; }
        public string DBName { get; set; }

        public InfluxDBStatus DBStatus { get; set; }

        public string MessageForAdmin { get; set; }
    }
}
