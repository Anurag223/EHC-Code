using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Tlm.Sdk.Api;
using Tlm.Sdk.Api.Configurers;
using Tlm.Sdk.AspNetCore;

namespace TLM.EHC.API
{
    [ExcludeFromCodeCoverage]
    public class EhcApiDocumentationConfigurer : ApiVersioningAndDocumentationConfigurer
    {
        protected override void MvcSetup(MvcOptions options, Startup startup)
        {
            options.EnableEndpointRouting = false;

            var config = startup.GetConfig<DocumentationConfig>();
            if (config.Options.HasFlag(ExtendedDocumentOptions.AllExceptHypermedia))
                AddJsonAndJsonApiOutputFormatters(options);

            if (config.Options.HasFlag(ExtendedDocumentOptions.SupportsInclusion))
                AddSubresourceStrippingResultFilter(options);
           
            AddQueryModelBinderProvider(options);
            AddExceptionFilter(options);
            RespectAcceptHeader(options);
        }
    }
}