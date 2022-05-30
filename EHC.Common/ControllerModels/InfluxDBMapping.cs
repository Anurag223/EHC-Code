using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Metadata;

namespace TLM.EHC.Admin
{
    /// <summary>
    /// InfluxDBMapping object which is used for get/post
    ///  </summary>
    [IsRoot]
    [ExcludeFromCodeCoverage]
    public class InfluxDBMapping : Entity
    {
        public string DbName { get; set; }
        public string RetentionPolicy { get; set; } = "autogen";

        public string MeasurementName { get; set; }
        public string BrandName { get; set; }

        public string BrandCode { get; set; }

        public string TechnologyCode { get; set; }

        public string TechnologyName { get; set; }

        [BsonRepresentation(BsonType.String)]
        public InfluxDBStatus Status { get; set; }

        public List<string> EquipmentCodes { get; set; }

    }
}
