using System.Diagnostics.CodeAnalysis;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Metadata;

// ReSharper disable once CheckNamespace
namespace TLM.EHC.Admin
{
    /// <summary>
    /// Model class corresponding to Collection name to maintain all audit logs from A2R Utils UI
    /// </summary>
    [IsRoot]
    [ExcludeFromCodeCoverage]
    public class A2RUtilsAuditLog : Entity

    {
        /// <summary>
        /// Activity(action) type in A2R Utils UI
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public A2RUtilsActivityType ActivityType { get; set; }

        /// <summary>
        /// Application name in A2R Utils UI
        /// </summary>
        [BsonRepresentation(BsonType.String)]
        public A2RUtilsApplicationType ApplicationName { get; set; }
        /// <summary>
        /// Old value before action was performed on A2R Utils UI
        /// </summary>
        public string OldValue { get; set; }
        /// <summary>
        ///  New value after action was performed on A2R Utils UI
        /// </summary>
        public string NewValue { get; set; }
      
    }
}
