using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.WritingData;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// To show the example for Channels bulk update 
    /// </summary>
    public class BulkRowsRequest
    {
        /// <summary>
        /// Equipment wkeid mappings.
        /// </summary>
        public List<EquipmentWkeMappings> EquipmentWkeMappings { get; set; }
        /// <summary>
        /// channel body data in json format.
        /// </summary>
        public BulkChannelsBody Body { get; set; }

    }
    /// <summary>
    /// Equipment wkeid mappings data 
    /// </summary>
    public class EquipmentWkeMappings
    {
        /// <summary>
        /// Device id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// EquipmentWkeid-All equipment are traceable
        /// and trackable via a Serial Number - Equipment Code combination.
        /// </summary>
        public string EquipmentWkeId { get; set; }
    }

    /// <summary>
    /// To provide the example for bulk update for channel body
    /// </summary>
    public class BulkChannelsBody
    {
        /// <summary>
        /// list of channels
        /// </summary>
        public List<ChannelRequest> Channels { get; set; }

        /// <summary>
        /// list of data in bulkdata format
        /// </summary>
        public List<BulkData> Data { get; set; }

    }

    /// <summary>
    /// Data format for channels bulk update for showing the example
    /// </summary>
    public class BulkData
    {
        /// <summary>
        /// Device Id
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Data for storing in influx db
        /// </summary>
        public List<List<object>> Rows { get; set; }

        /// <summary>
        /// Equipment Wkeid
        /// </summary>
        public string EquipmentWkeId { get; set; }

    }
}
