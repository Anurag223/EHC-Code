using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Exceptions
{
    public abstract class HttpStatusException : EhcApiException
    {
        protected HttpStatusException(string message) : base(message)
        {
        }

        protected HttpStatusException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
