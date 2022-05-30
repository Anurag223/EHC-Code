using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace TLM.EHC.API.Formatters
{
    public class ActualJsonFormatter : AbstractJsonFormatter
    {
        public ActualJsonFormatter(SdkFormatter sdkFormatter): base(sdkFormatter)
        {
            var parsed = MediaTypeHeaderValue.Parse("application/json");
            this.AddMediaType(parsed);
        }
    }
}
