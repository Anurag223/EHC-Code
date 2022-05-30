using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TLM.EHC.Common.Historian;

namespace TLM.EHC.API.HealthCheck
{
    public class HealthCheckInfluxDb : IHealthCheck
    {
        private readonly IHistorianClient _historianClient;

        public HealthCheckInfluxDb(IHistorianClient historianClient)
        {
            _historianClient = historianClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            var databases = await _historianClient.ShowDatabases();
            return HealthCheckResult.Healthy("Database count: " + databases.Values.Count);
        }
    }
}
