using System.Diagnostics.CodeAnalysis;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Tlm.Sdk.Api;
using Tlm.Sdk.Api.Configurers;
using Tlm.Sdk.AspNetCore;
using TLM.EHC.Admin;

namespace TLM.EHC.ADMIN.API
{
    [ExcludeFromCodeCoverage]
    public class ApiSupportConfigurer : ApiStrategiesConfigurer
    {

        protected override void ConfigureStrategies(ContainerBuilder builder)
        {

            SupportGetFromCache<InfluxDBMapping>(builder);
            SupportGetCollectionFromCache<InfluxDBMapping>(builder);           
        }
      
    }
}
