using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Format for getting data of channels
    /// </summary>
    public class BulkRows : ResponseEntity
    {
        /// <summary>
        /// List of EuipmentList tags in MultipleChannels format.
        /// </summary>
        public List<MultipleChannels> EquipmentList { get; set; }
        /// <summary>
        /// List of EquipmentWkeMappings
        /// </summary>

        [CanBeNull]
        public List<EquipmentWkeMappings> EquipmentWkeMappings { get; set; }

      
    }

    /// <summary>
    /// Episode rows including episodeid.
    /// </summary>
    public class EpisodeRows : ResponseEntity
    {
        /// <summary>
        /// Episode id
        /// </summary>
        public string EpisodeId { get; set; }

        public List<MultipleChannels> EquipmentList { get; set; }

    }

}
