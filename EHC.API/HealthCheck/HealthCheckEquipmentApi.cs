using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Clients.EquipmentApi;

namespace TLM.EHC.API.HealthCheck
{
    public class HealthCheckEquipmentApi : IHealthCheck
    {
        private readonly IEquipmentApiClient _equipmentApiClient;

        public HealthCheckEquipmentApi(IEquipmentApiClient equipmentApiClient)
        {
            _equipmentApiClient = equipmentApiClient;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            // NX4_CTS_SURFACE_EQUIPMENT
            // TUBING_CABIN_SKID_WS-86
            // TCS-323
            const string wkeid = "100298911:TCS32300Y0423";

            var equipment = await _equipmentApiClient.GetEquipmentByWkeId(wkeid); // can be null

            return HealthCheckResult.Healthy("Equipment description: " + equipment?.Description);
        }
    }
}
