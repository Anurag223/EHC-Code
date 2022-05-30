using System.Collections.Generic;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// This object will be returned when we want to get the channels,
    /// episodic points,Readings by code.
    /// </summary>
    public class SingleChannel : ResponseEntity
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

        // public string Code { get; set; }
        // public string Uom { get; set; }
        // public string Dimension { get; set; }
        /// <summary>
        /// Channel in the form of ChannelDefitionClean. 
        /// </summary>
        public ChannelDefinitionClean Channel { get; set; }

        /// <summary>
        /// Timeperiod with start and end.
        /// </summary>
        public TimePeriod Period { get; set; }
        /// <summary>
        /// List of rows in time series format.
        /// </summary>
        public List<List<object>> Rows { get; set; }
    }
}
