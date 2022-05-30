using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace TLM.EHC.API.Formatters
{
    public class InfluxCsvFormatter : TextOutputFormatter
    {
        public InfluxCsvFormatter()
        {
            var headerValue = MediaTypeHeaderValue.Parse("text/csv");

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);

            SupportedMediaTypes.Add(headerValue.CopyAsReadOnly());
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            // empty because we return raw content, no object serialization needed
            return Task.CompletedTask;
        }
    }
}
