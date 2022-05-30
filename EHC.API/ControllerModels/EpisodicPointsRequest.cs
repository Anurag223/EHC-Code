using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// To post data in episodic point this format should be used.
    /// </summary>
    public class EpisodicPointsRequest
    {
        /// <summary>
        /// Meta tag with the content of EpisodicMeta.
        /// </summary>
        public EpisodicMeta Meta { get; set; }
        /// <summary>
        /// List of data for time series storage.
        /// </summary>
        public List<List<object>> Rows { get; set; }
    }

    /// <summary>
    /// Details of Meta tag for posting data in episodic points.
    /// </summary>
    public class EpisodicMeta
    {
        /// <summary>
        /// Unique equipment Wkeid.
        /// </summary>
        public string EquipmentWkeId { get; set; }
        /// <summary>
        /// Episode Id.
        /// </summary>
        public string EpisodeId { get; set; }
        /// <summary>
        /// Material number.
        /// </summary>
        public string MaterialNumber { get; set; }
        /// <summary>
        /// Serial number.
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// Equipment code.
        /// </summary>
        public string EquipmentCode { get; set; }
        /// <summary>
        /// time period mentioning start and end.
        /// </summary>
        public TimePeriod Period { get; set; }
        /// <summary>
        /// List of channelrequest object.
        /// </summary>
        public List<ChannelRequest> Channels { get; set; }
    }
        
    
}
