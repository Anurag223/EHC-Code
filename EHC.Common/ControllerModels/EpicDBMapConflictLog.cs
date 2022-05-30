using System;
using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Metadata;

namespace TLM.EHC.Admin
{
    [IsRoot]
    [ExcludeFromCodeCoverage]
    public class EpicDBMapConflictLog : Entity
    {
        public string EpicBrandName { get; set; }
        public string DBMapBrandName { get; set; }
        public string EpicTechnologyName { get; set; }
        public string DBMapTechnologyName { get; set; }
        public string EpicBrandCode { get; set; }
        public string DBMapBrandCode { get; set; }
        public string EpicTechnologyCode { get; set; }
        public string DBMapTechnologyCode { get; set; }
        public string EpicEquipmentCode { get; set; }
        public string DBMapEquipmentCode { get; set; }
         public string ConflictStartDate { get; set; }
        [BsonRepresentation(BsonType.String)]
        public DBMapConflictStatus ConflictStatus { get; set; }
    }
}
