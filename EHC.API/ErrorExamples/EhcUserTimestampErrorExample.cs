using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.ErrorExamples
{
    public static class EhcUserTimestampErrorExample
    {
        public static Error InvalidThresholdTimestamp(string key, string thresholdtimestampdate) => Error.InvalidParameter(key,
            "Invalid threshold timestamp", String.Format(EhcConstants.InvalidThresholdTimestamp, thresholdtimestampdate));

    }
}
