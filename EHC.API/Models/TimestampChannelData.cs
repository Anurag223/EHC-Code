using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Tlm.Sdk.Core.Models;
using Tlm.Sdk.Core.Models.Metadata;

namespace TLM.EHC.API.Models
{
    [ExcludeFromCodeCoverage]
    public class TimestampChannelData
    {
        public string Code { get; set; }

        public string ThresholdTimestamp { get; set; }
        [CanBeNull]
        public string LatestTimestamp { get; set; }

    }
}
