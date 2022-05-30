using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.API.Common;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ErrorExamples
{
    public static class EhcChannelDefinitionErrorExample
    {
        public static Error ChannelDefinitionNotFound(string key, string code)
        {
            return
                new Error((string)null, (object)404, (Link)null, (string)null,
                    "Channel definition not found", String.Format(EhcConstants.ChannelDefinitionNotFound, code),
                    new ErrorSource((string)null, key),
                    (IDictionary<string, object>)null);
        }
    }

}

