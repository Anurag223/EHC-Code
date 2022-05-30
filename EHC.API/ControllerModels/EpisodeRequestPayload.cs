using System;
using Tlm.Sdk.Core.Models.Hypermedia;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.API.ControllerModels.Separated;

namespace TLM.EHC.API.ControllerModels
{

    public class EpisodeRequestPayload : AttributedCmmsTrackedResource
    {
        public string Name { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public string Type { get; set; }


        public QuerySpec GetQueryForRepresentation()
        {
            return SpecBuilder
                .ForQuery<Episode>()
                .By(x => x.Name, Name)
                .By(x => x.Type, Type);
        }
    }
}
