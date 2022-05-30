using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Tlm.Fed.Framework.ApiSupport.Configurers;
using Tlm.Fed.Framework.Common.Configurers;
using Tlm.Sdk.Api;
using Tlm.Sdk.Api.Configurers;
using Tlm.Sdk.AspNetCore;
using Tlm.Sdk.AspNetCore.Configurers;
using TLM.EHC.ADMIN.API;
using TLM.EHC.Common;

namespace EHC.ADMIN.API
{
    /// <summary>
    /// Startup class for EHC admin API
    /// </summary>
    public class StartupForEhcAdminApi : StartupForSdkApi
    {
        public StartupForEhcAdminApi(IWebHostEnvironment env) : base(env)
        {
            AddConfigurer(new MongoDbConfigurer());
            AddConfigurer(new SecurityConfigurer());
            AddConfigurer(new StronglyTypedConfigurationConfigurer<ApiConfig>());         
            AddConfigurer(new ApiConfigurer());
            AddConfigurer(new ApiSupportConfigurer());
        }
    }
    [ExcludeFromCodeCoverage]
    public class Program : ProgramForMicroservice<StartupForEhcAdminApi>
    {
        public static async Task Main(string[] args) => await BaseMainAsync(args);
    }
}
