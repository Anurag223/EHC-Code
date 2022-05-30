using Microsoft.AspNetCore.Server.Kestrel.Core;
using Tlm.Sdk.Data.Mongo;
using Tlm.Sdk.Api;
using System.Diagnostics.CodeAnalysis;
using Tlm.Sdk.AspNetCore;
using Autofac;
using Microsoft.Extensions.DependencyInjection;
using TLM.EHC.Admin;
using TLM.EHC.Common.Clients.EpicV3Api;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Services;
using TLM.EHC.ADMIN.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.ADMIN.API.Services;

namespace TLM.EHC.ADMIN.API
{
    [ExcludeFromCodeCoverage]
    public class ApiConfigurer : Configurer
    {
        public override string Name => "EhcAdminApiConfigurer";

        public override void ConfigureServicesViaAutofac(Startup startup, ContainerBuilder builder)
        {           
            builder.AddRepoSupport<InfluxDBMapping>();
            builder.AddRepoSupport<EpicDBMapConflictLog>();
            builder.AddRepoSupport<A2RUtilsAuditLog>();
        }

        public override void ConfigureServices(Startup startup, IServiceCollection services)
        {           
            services.AddMemoryCache();
            services.AddSingleton<IHistorianClient, HistorianClient>();
            services.AddSingleton<IUrlBuilder, UrlBuilder>();
            services.AddSingleton<IInfluxDBMappingService, InfluxDBMappingService>();
            services.AddSingleton<IDBMapConflictLogService, DBMapConflictLogService>();
            services.AddSingleton<IA2RUtilsAuditLogService, A2RUtilsAuditLogService>();
            services.AddSingleton<IAdminApiImplementation, AdminApiImplementation>();
            services.AddHttpClient();
            // only needed when no nginx in front
            services.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue;
            });
            services.AddSingleton<IEpicV3ApiClient, EpicV3ApiClient>();
            services.AddSingleton<IEpicV3HierarchyProvider, EpicV3HierarchyProvider>();
            services.AddSingleton<ICurrentTime, CurrentTimeProvider>();           
        }


    }
}
