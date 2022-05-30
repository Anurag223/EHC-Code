using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TLM.EHC.Common.Clients.EquipmentModelApi;

namespace TLM.EHC.API.HealthCheck
{
    public class HealthCheckEquipmentModelApi : IHealthCheck
    {
        private readonly IEquipmentModelApiClient _equipmentModelApiClient;

        public HealthCheckEquipmentModelApi(IEquipmentModelApiClient equipmentModelApiClient)
        {
            _equipmentModelApiClient = equipmentModelApiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            const string equipmentCode = "TCS-323";

            var model = await _equipmentModelApiClient.GetByEquipmentCodeAsync(equipmentCode, null);

            return HealthCheckResult.Healthy("Equipment model description: " + model.Description);
        }
    }
}
