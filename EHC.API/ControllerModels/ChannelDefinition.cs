using System.Collections.Generic;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Metadata;
using Tlm.Sdk.Core.Models.Querying;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Channel Definition object which is used for get/put/post
    ///  </summary>
    [IsRoot]
    public class ChannelDefinition : Entity
    {
        /// <summary>
        /// Channel definition Code
        ///  </summary>
        [CanBeFilteredOn]
        public string Code { get; set; }

        /// <summary>
        /// Channel definition Name
        ///  </summary>
        [CanBeFilteredOn]
        public string Name { get; set; }

        /// <summary>
        /// Channel definition dimension
        ///  </summary>
        [CanBeFilteredOn]
        public string Dimension { get; set; }

        /// <summary>
        /// Channel definition unit of measure
        ///  </summary>
        [CanBeFilteredOn]
        public string Uom { get; set; }

        /// <summary>
        /// Channel definition type
        ///  </summary>
        [CanBeFilteredOn]
        public string Type { get; set; }

        /// <summary>
        /// Channel definition legal classification
        ///  </summary>
        [CanBeFilteredOn]
        public string LegalClassification { get; set; }

        /// <summary>
        /// Channel definition Equipment codes
        ///  </summary>
        public List<string> EquipmentCodes { get; set; }
    }


    public class ChannelDefinitionIndex
    {
        public ChannelDefinition ChannelDefinition { get; }
        public int? Index { get; }

        public ChannelDefinitionIndex(ChannelDefinition channelDefinition, int? index)
        {
            this.ChannelDefinition = channelDefinition;
            this.Index = index;
        }
    }


    // because we don't want system fields get serialized to json
    /// <summary>
    /// Channel Definition clean Object to not serialize the system fields onto json
    ///  </summary>
    public class ChannelDefinitionClean
    {
        /// <summary>
        /// Channel definition Code
        ///  </summary>
        public string Code { get; set; }
        /// <summary>
        /// Channel definition name
        ///  </summary>
        public string Name { get; set; }
        /// <summary>
        /// Channel definition dimension
        ///  </summary>
        public string Dimension { get; set; }
        /// <summary>
        /// Channel definition unit of measure
        ///  </summary>
        public string Uom { get; set; }
        /// <summary>
        /// Channel definition type
        ///  </summary>
        public string Type { get; set; }
        /// <summary>
        /// Channel definition legal classification
        ///  </summary>
        public string LegalClassification { get; set; }

        /// <summary>
        /// Channel definition index
        ///  </summary>
        public int? Index { get; set; }

        public List<string> EquipmentCodes { get; set; }

        public ChannelDefinitionClean()
        {
        }

        public ChannelDefinitionClean(ChannelDefinition channelDefinition)
        {
            this.Code = channelDefinition.Code;
            this.Name = channelDefinition.Name;
            this.Dimension = channelDefinition.Dimension;
            this.Uom = channelDefinition.Uom;
            this.Type = channelDefinition.Type;
            this.LegalClassification = channelDefinition.LegalClassification;
            this.EquipmentCodes = channelDefinition.EquipmentCodes;
        }
    }
}
