using System.Diagnostics.CodeAnalysis;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.API.Formatters;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.HyperLinks;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.API.Services;
using TLM.EHC.API.WritingData;
using Tlm.Sdk.AspNetCore;
using ChannelDefinition = TLM.EHC.API.ControllerModels.ChannelDefinition;
using Episode = TLM.EHC.API.ControllerModels.Separated.Episode;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Tlm.Sdk.Data.Mongo;
using Tlm.Sdk.Api;
using TLM.EHC.Admin;
using TLM.EHC.Common.Clients.EpicV3Api;
using TLM.EHC.Common.Clients.EquipmentApi;
using TLM.EHC.Common.Clients.EquipmentModelApi;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Services;
using TLM.EHC.Common;
using Microsoft.AspNetCore.Builder;
using Anemonis.AspNetCore.RequestDecompression;
using Tlm.Sdk.Core;

namespace TLM.EHC.API
{
    [ExcludeFromCodeCoverage]  
    public class ApiConfigurer : Configurer
    {
        public override string Name => "EhcApiConfigurer";

        public override void ConfigureServicesViaAutofac(Startup startup, ContainerBuilder builder)
        {
            builder.RegisterMaybe(
                "UriPartProviders",
                () =>
                {
                    builder.RegisterSingleton<MateoOriginsHeadersUriPartProvider, IUriPartProvider>();
                    builder.RegisterSingleton<ForwardedHeadersUriPartProvider, IUriPartProvider>();
                    builder.RegisterSingleton<XForwardedHeadersUriPartProvider, IUriPartProvider>();
                    builder.RegisterSingleton<XOriginalHeadersUriPartProvider, IUriPartProvider>();
                    builder.RegisterSingleton<EHCConfigUriPartProvider, IUriPartProvider>();
                    builder.RegisterSingleton<HttpRequestUriPartProvider, IUriPartProvider>();
                });
            builder.AddRepoSupport<Episode>();
            builder.RegisterType<EpisodeHypermediaLinker>().As<IHypermediaLinker<Episode>>();
            builder.AddRepoSupport<ChannelDefinition>();
            builder.AddRepoSupport<InfluxDBMapping>();
        }

        public override void Configure(Startup startup, IApplicationBuilder app)
        {
            app.UseRequestDecompression();
        }


        public override void ConfigureServices(Startup startup, IServiceCollection services)
        {
            services.AddMvc(OutputFormatters.Configure);
            services.AddMemoryCache();

            services.AddSingleton<ICurrentTime, CurrentTimeProvider>();

            services.AddSingleton<IHistorianClient, HistorianClient>();
            services.AddSingleton<IResponseProviderResolver, ResponseProviderResolver>();
            services.AddSingleton<IUrlBuilder, UrlBuilder>();
            services.AddSingleton<IApiImplementation, ApiImplementation>();
            services.AddSingleton<IHyperLinksProvider, HyperLinksProvider>();
            services.AddSingleton<IDataParser, DataParser>();
            services.AddSingleton<IDataMapper, DataMapper>();
            services.AddSingleton<IHistorianWriter, HistorianWriter>();
            services.AddSingleton<ITimestampParser, TimestampParser>();
            services.AddSingleton<ISecurityTokenProvider, SecurityTokenProvider>();

            services.AddSingleton<IEpisodicPointService, EpisodicPointService>();
            services.AddSingleton<IEpisodeService, EpisodeService>();
            services.AddSingleton<IChannelDefinitionService, ChannelDefinitionService>();
            services.AddSingleton<IInfluxDBMappingService, InfluxDBMappingService>();

            services.AddSingleton<IEquipmentApiClient, EquipmentApiClient>();
            services.AddSingleton<IEquipmentModelApiClient, EquipmentModelApiClient>();
            services.AddSingleton<IEpicV3ApiClient, EpicV3ApiClient>();

            services.AddSingleton<TimestampParser>();

            services.AddSingleton<ResponseProviderV1>();
            services.AddSingleton<ResponseProviderSingleChannel>();
            services.AddSingleton<ResponseProviderMultipleChannels>();
            services.AddSingleton<ResponseProviderInflux>();
            services.AddSingleton<ResponseProviderCsv>();

            services.AddSingleton<IEquipmentProvider, MateoEquipmentProvider>();
            services.AddSingleton<IEquipmentModelProvider, EquipmentModelProvider>();
            services.AddSingleton<IEpicV3HierarchyProvider, EpicV3HierarchyProvider>();

            services.AddSingleton<IChannelProvider, ChannelProvider>();

            services.AddHttpClient();

            services.AddRequestDecompression(o =>
            {
                o.Providers.Add<DeflateDecompressionProvider>();
                o.Providers.Add<GzipDecompressionProvider>();
                o.Providers.Add<BrotliDecompressionProvider>();
            });

            // only needed when no nginx in front
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });
        }


    }
}
