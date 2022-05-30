using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Exceptions
{
    public class BadRequestException : HttpStatusException
    {
      
        public BadRequestException(string message) : base(message)
        {
           
        }

        public BadRequestException(string message, Exception ex) : base(message, ex)
        {
        }
    }
}
