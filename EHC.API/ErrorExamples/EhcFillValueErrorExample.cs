using System;
using JetBrains.Annotations;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.ErrorExamples
{
    public class EhcFillValueErrorExample
    {

        public static Error InvalidFillValue(string key, [NotNull] string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            return Error.InvalidParameter(key,
                "Invalid Fill Value", string.Format(EhcConstants.InvalidFillValue));
        }
    }
}
