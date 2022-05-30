using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Hypermedia;
using Tlm.Sdk.Core.Models.Metadata;
using Tlm.Sdk.Core.Models.Querying;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Channel request to show the example.
    /// </summary>

    public class ChannelRequest
    {
        /// <summary>
        /// Index.
        /// </summary>
        public int? Index { get; set; }
        /// <summary>
        /// Code.
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Unit of measure.
        /// </summary>
        public string Uom { get; set; }
        /// <summary>
        /// Dimension.
        /// </summary>
        public string Dimension { get; set; }


    }
}
