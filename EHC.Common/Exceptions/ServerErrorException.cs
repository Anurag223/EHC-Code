using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Exceptions
{
    public class ServerErrorException : HttpStatusException
    {
        public ServerErrorException(string message) : base(message)
        {
        }

        public ServerErrorException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
