using System.Diagnostics.CodeAnalysis;
using Anemonis.AspNetCore.RequestDecompression;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tlm.Sdk.Api.Configurers;
using Tlm.Sdk.AspNetCore;
using Tlm.Sdk.Data.Mongo;
using TLM.EHC.API.Swagger;
using ChannelDefinition = TLM.EHC.API.ControllerModels.ChannelDefinition;
using Episode = TLM.EHC.API.ControllerModels.Separated.Episode;

namespace TLM.EHC.API
{
    [ExcludeFromCodeCoverage]
    public class ApiSupportConfigurer : ApiStrategiesConfigurer
    {
        protected override void ConfigureStrategies(ContainerBuilder builder)
        {

            SupportGetFromCache<Episode>(builder);
            SupportGetCollectionFromCache<Episode>(builder);
            SupportGetFromCache<ChannelDefinition>(builder);
            SupportGetCollectionFromCache<ChannelDefinition>(builder);
            HostingConfig.Instance.Providers = new[] { UriPartProviderId.ForwardedHeaders, UriPartProviderId.MateoOriginsHeaders,
                UriPartProviderId.Config, UriPartProviderId.XForwardedHeaders, UriPartProviderId.XOriginalHeaders, UriPartProviderId.HttpRequest };


        }

    }
}
