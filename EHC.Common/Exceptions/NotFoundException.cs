using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Exceptions
{
    public class NotFoundException : HttpStatusException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
