using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Hypermedia;
using Tlm.Sdk.Core.Models.Metadata;
using Tlm.Sdk.Core.Models.Querying;

namespace TLM.EHC.API.ControllerModels.Separated
{
    /// <summary>
    /// An entity as Episode 
    /// </summary>
    [IsRoot]
    public class Episode : Entity
    {
        /// <summary>
        /// Episode name.
        /// </summary>
        [CanBeFilteredOn]
        public string Name { get; set; }

        /// <summary>
        /// Start time of episode.
        /// </summary>
        [CanBeFilteredOn]
        public DateTime StartTime { get; set; }

        /// <summary>
        /// End time of episode 
        /// </summary>
        [CanBeFilteredOn]
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        [CanBeFilteredOn]
        public string Type { get; set; }

        /// <summary>
        /// Parent Id
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// List of tags.
        /// </summary>
        public List<string> Tags { get; set; }

        /// <summary>
        /// List of equipmet Wkeid
        /// </summary>
        public List<string> EquipmentWkeIdList { get; set; }

        /// <summary>
        /// Key value pair of episode relationship.
        /// </summary>
        public Dictionary<string, EpisodeRelationship> Relationships { get; set; }

        public Episode()
        {
            Tags = new List<string>();
            EquipmentWkeIdList = new List<string>();
            Relationships = new Dictionary<string, EpisodeRelationship>();
        }
    }

    /*
    InvalidOperationException: Conflicting schemaIds: Identical schemaIds detected for types
    TLM.EHC.API.Models.Relationship
    and
    Tlm.Fed.Framework.Core.Models.Hypermedia.Relationship.
    See config settings - "CustomSchemaIds" for a workaround
    Swashbuckle.AspNetCore.SwaggerGen.SchemaIdManager.IdFor(Type type)
    */



    /// <summary>
    /// EpisodeRelationship
    /// </summary>
    public class EpisodeRelationship
    {
        public Dictionary<string, EpisodeLink> Links { get; set; }
        public Dictionary<string, string> Data { get; set; }

        public EpisodeRelationship()
        {
            Links = new Dictionary<string, EpisodeLink>();
            Data = new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// EpisodeLink
    /// </summary>
    public class EpisodeLink
    {
        public string Href { get; set; }
        public string Rel { get; set; }
    }
}
