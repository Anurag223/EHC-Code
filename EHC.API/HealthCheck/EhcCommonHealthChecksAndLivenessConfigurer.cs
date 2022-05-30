using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tlm.Sdk.AspNetCore;
using Tlm.Sdk.AspNetCore.Configurers;
using Tlm.Sdk.Data.Mongo;

namespace TLM.EHC.API.HealthCheck
{
    /// <summary>
    /// This class is created because CommonHealthChecksAndLivenessConfigurer class from framework takes RabbitMQ also
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class EhcCommonHealthChecksAndLivenessConfigurer : HealthChecksAndLivenessConfigurer
    {
        protected override void ConfigureHealthChecks(
            Startup startup,
            IServiceCollection services,
            IHealthChecksBuilder builder)
        {
            builder.AddMongoDbHealthCheck(startup.GetConfig<RepositoryConfig>((RepositoryConfig)null), (string)null, new HealthStatus?(), (IEnumerable<string>)null);
            
        }

    }
}