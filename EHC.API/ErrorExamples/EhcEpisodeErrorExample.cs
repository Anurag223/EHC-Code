using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ErrorExamples
{
    public static class EhcEpisodeErrorExample
    {
        public static Error EpisodeNotFound(string key, string id)
        {
            return
                new Error((string)null, (object)404, (Link)null, (string)null,
                    "Episode not found", String.Format(EhcConstants.EpisodeNotFound, id),
                    new ErrorSource((string)null, key),
                    (IDictionary<string, object>)null);
        }
    }
}
