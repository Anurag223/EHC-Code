using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace TLM.EHC.API.Formatters
{
    public class LegacyJsonFormatter : AbstractJsonFormatter
    {
        public LegacyJsonFormatter(SdkFormatter sdkFormatter) : base(sdkFormatter)
        {
            var parsed = MediaTypeHeaderValue.Parse("application/vnd.v1+json");
            this.AddMediaType(parsed);
        }
    }
}
