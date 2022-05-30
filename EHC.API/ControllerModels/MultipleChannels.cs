using System.Collections.Generic;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Gets data from Influxdb used as response type for Channels,Readings,Episodic-point
    /// </summary>
    public class MultipleChannels : ResponseEntity
    {
        public MultipleChannelsMeta Meta { get; set; }
        public List<List<object>> Rows { get; set; }
    }

    /// <summary>
    /// Sub data of multiple channels
    /// </summary>
    public class MultipleChannelsMeta
    {
        /// <summary>
        /// Unique equipment Id(Equipment Id=MaterialNumber:SerialNumber).
        /// </summary>
        public string EquipmentWkeId { get; set; }
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
        /// Timeperiod mentioning start and end.
        /// </summary>
        public TimePeriod Period { get; set; }
        /// <summary>
        /// list of data in ChannelDefinitionClean format.
        /// </summary>
        public List<ChannelDefinitionClean> Channels { get; set; }
    }
}
