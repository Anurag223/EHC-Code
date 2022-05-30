using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Hypermedia;
using Tlm.Sdk.Core.Models.Metadata;
using Tlm.Sdk.Core.Models.Querying;
using  TLM.EHC.API.ControllerModels.Separated;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Episode Request object for create and update episode.
    /// </summary>

    public class EpisodeRequest : Episode
    {
        /// <summary>
        /// List of data in EpisodeData format.
        /// </summary>
        [BsonIgnore]
        public List<EpisodeData> Data { get; set; }

        /// <summary>
        /// List of channels in ChannelRequest format.
        /// </summary>
        [BsonIgnore]
        public List<ChannelRequest> Channels { get; set; }

    }

    /// <summary>
    /// Format of episode data 
    /// </summary>
    public class EpisodeData
    {
        /// <summary>
        /// Unique Euipment id.
        /// </summary>
        public string EquipmentWkeId { get; set; }

        /// <summary>
        /// List of Rows for time series storage.
        /// </summary>
       public List<List<object>> Rows { get; set; }

    }
}
