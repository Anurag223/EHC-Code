using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Exceptions
{
    public interface ICurrentTime
    {
        DateTime Now { get; }
    }

    [ExcludeFromCodeCoverage]
    public class CurrentTimeProvider : ICurrentTime
    {
        public DateTime Now => DateTime.Now;
    }
}
