using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using Tlm.Sdk.Api;

namespace TLM.EHC.API.Formatters
{
    public static class OutputFormatters
    {
        public static void Configure(MvcOptions options)
        {
            var hypermediaFormatter = options.OutputFormatters.OfType<HypermediaPassthroughJsonFormatter>().Single();

            var sdkFormatter = new SdkFormatter();
            sdkFormatter.MvcOptions = options;

            var newtonsoftFormatter = hypermediaFormatter.GetType()
               .GetField("_actualJsonFormatter", BindingFlags.Instance | BindingFlags.NonPublic)
               .GetValue(hypermediaFormatter) as NewtonsoftJsonOutputFormatter;

            sdkFormatter.SerializerSettings = newtonsoftFormatter
                .GetType()
                .GetProperty("SerializerSettings", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(newtonsoftFormatter) as JsonSerializerSettings;


            options.OutputFormatters.Clear();
            options.OutputFormatters.Add(new ActualJsonFormatter(sdkFormatter));         
           
           
        }
    }

    public class SdkFormatter
    {
        public MvcOptions MvcOptions { get; set; }
        public JsonSerializerSettings SerializerSettings { get; set; }
    }
}
