
using System.Diagnostics.CodeAnalysis;

namespace TLM.EHC.ADMIN.API.ControllerModels
{
    [ExcludeFromCodeCoverage]
    public class InfluxAndDbMappingUpdateResponse
    {
        public bool DbMapUpdateStatus { get; set; }

        public string DbMapUpdateMessage { get; set; }

        public string InfluxDbCreationMessage { get; set; }

        public string ErrorDetails { get; set; }

    }
}
