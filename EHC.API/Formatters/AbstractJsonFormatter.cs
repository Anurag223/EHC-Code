using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;
using System.Buffers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.Formatters
{
    public abstract class AbstractJsonFormatter : TextOutputFormatter
    {
        private readonly NewtonsoftJsonOutputFormatter _actualJsonFormatter;

        public AbstractJsonFormatter(SdkFormatter sdkFormatter)
        {
            _actualJsonFormatter = new NewtonsoftJsonOutputFormatter(sdkFormatter.SerializerSettings, ArrayPool<char>.Shared, sdkFormatter.MvcOptions);
            _actualJsonFormatter.SupportedMediaTypes.Clear();

            SupportedEncodings.Add(Encoding.UTF8);
            SupportedEncodings.Add(Encoding.Unicode);
            
        }

        protected void AddMediaType(MediaTypeHeaderValue mediaType)
        {
            _actualJsonFormatter.SupportedMediaTypes.Add(mediaType.CopyAsReadOnly());
            SupportedMediaTypes.Add(mediaType.CopyAsReadOnly());
        }

        public override bool CanWriteResult(OutputFormatterCanWriteContext context)
        {
            return _actualJsonFormatter.CanWriteResult(context);
        }

        public override Task WriteAsync(OutputFormatterWriteContext context)
        {
            StripHypermedia(context);
            return _actualJsonFormatter.WriteAsync(context);
        }

        public override Task WriteResponseBodyAsync(OutputFormatterWriteContext context, Encoding selectedEncoding)
        {
            StripHypermedia(context);
            return _actualJsonFormatter.WriteResponseBodyAsync(context, selectedEncoding);
        }

        // THIS METHOD IS COPY-PASTE FROM HypermediaStrippingJsonFormatter

        private void StripHypermedia(OutputFormatterCanWriteContext context)
        {
            if (!(context.Object is HypermediaDocument doc))
                // this handles cases where we have APIs that have nothing to do with hypermedia and cases like 404, etc.
                return;

            // replace the hypermedia document with the actual resource
            var @object = doc.GetRawData();

            context
                .GetType()
                .GetProperty(nameof(OutputFormatterWriteContext.Object), BindingFlags.Instance | BindingFlags.Public)
                .SetValue(context, @object);

            context
                .GetType()
                .GetProperty(nameof(OutputFormatterWriteContext.ObjectType), BindingFlags.Instance | BindingFlags.Public)
                .SetValue(context, @object.GetType());
        }
    }
}
