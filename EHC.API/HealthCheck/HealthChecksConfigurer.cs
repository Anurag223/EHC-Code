using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Tlm.Fed.Framework.Common.Configurers;
using Tlm.Sdk.AspNetCore;

namespace TLM.EHC.API.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public class HealthChecksConfigurer : EhcCommonHealthChecksAndLivenessConfigurer
    {
        protected override void ConfigureHealthChecks(Startup startup, IServiceCollection services, IHealthChecksBuilder builder)
        {
            try
            {
                // exception happens, but we just need ignore that
                base.ConfigureHealthChecks(startup, services, builder);
            }
            finally
            {
                builder.AddCheck<HealthCheckInfluxDb>("connection_to_influx_db");
            }
        }
    }
}
