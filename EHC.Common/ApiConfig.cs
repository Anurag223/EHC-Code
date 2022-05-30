using System;
using System.Diagnostics.CodeAnalysis;
using Tlm.Fed.Framework.ApiSupport;

namespace TLM.EHC.Common
{
    [ExcludeFromCodeCoverage]
    public class ApiConfig : FrameworkApiSupportConfig
    {
        public EhcApiConfig EhcApiConfig { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EhcApiConfig
    {
        public ExternalApi InfluxDB { get; set; }
        public ExternalApi EquipmentApi { get; set; }
        public ExternalApi EquipmentModelApi { get; set; }
        public ExternalApi EpicV3Api { get; set; }
        public ExternalApi OdmApi { get; set; }

        public string EhcSupportEmail { get; set; }
        public double ServiceCacheTimeDuration { get; set; }

    }

    [ExcludeFromCodeCoverage]
    public class ExternalApi
    {
        public string BaseUrl { get; set; }

        // for basic auth
        public string Username { get; set; }
        public string Password { get; set; }

        // for apigee auth
        public string XApiKey { get; set; }

        // for identity server auth
        public string TokenAddress { get; set; }
        public string TokenScope { get; set; }
        public string TokenClientId { get; set; }
        public string TokenClientSecret { get; set; }
        public int TokenCachingInMinutes { get; set; }

        public double CacheTimeDuration { get; set; }
    }
}
