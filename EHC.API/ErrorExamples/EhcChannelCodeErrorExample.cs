using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.API.Common;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ErrorExamples
{
    public static class EhcChannelCodeErrorExample
    {
        public static Error InvalidChannelCode(string key, string code) => Error.InvalidParameter(key,
            "Invalid Channel Code", String.Format(EhcConstants.InvalidChannelCode, code));

        public static Error ChannelCodeNotFound(string key, string code)
        {
            return
                new Error((string)null, (object)404, (Link)null, (string)null,
                    "ChannelCodeNotFound", String.Format(EhcConstants.ChannelCodeNotFound, code),
                    new ErrorSource((string)null, key),
                    (IDictionary<string, object>)null);
        }

    }

}

