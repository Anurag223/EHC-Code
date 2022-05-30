using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using TLM.EHC.API.HealthCheck;
using Tlm.Fed.Framework.ApiSupport.Configurers;
using Tlm.Fed.Framework.Common.Configurers;
using Tlm.Sdk.Api;
using Tlm.Sdk.AspNetCore;
using Tlm.Sdk.AspNetCore.Configurers;
using TLM.EHC.Common;

namespace TLM.EHC.API
{
    [ExcludeFromCodeCoverage]   
    public class StartupForEhcApi : StartupForSdkApi
    {
        public StartupForEhcApi(IWebHostEnvironment env) : base(env)
        {           

            InsertConfigurer(new AdditionalConfigFilesConfigurer());
            AddConfigurer(new MongoDbConfigurer());          
            AddConfigurer(new SecurityConfigurer());           

            AddConfigurer(new HealthChecksConfigurer());

            AddConfigurer(new StronglyTypedConfigurationConfigurer<ApiConfig>());
            AddConfigurer(new EhcApiDocumentationConfigurer());
            AddConfigurer(new ApiConfigurer());
            AddConfigurer(new ApiSupportConfigurer());
        }
    }

    [ExcludeFromCodeCoverage]
    public class Program : ProgramForMicroservice<StartupForEhcApi>
    {
        public static async Task Main(string[] args) => await BaseMainAsync(args);
    }
}
