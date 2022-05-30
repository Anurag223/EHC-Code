using System.Collections.Generic;
using System.Linq;
using TLM.EHC.API.Controllers;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using  TLM.EHC.API.ControllerModels.Separated;
using Tlm.Sdk.AspNetCore;

namespace TLM.EHC.API.ControllerModels
{
    public class EpisodeHypermediaLinker : HypermediaLinker<Episode>     {
        private readonly LinkBuilder _linkBuilder;
        

        public EpisodeHypermediaLinker(IUriBuilder uriBuilder): base(uriBuilder)
        {           
            _linkBuilder = new LinkBuilder(uriBuilder);
        }

        public Links BuildResourceLinks(Episode item)
        {
            if (item == null) return new Links(Enumerable.Empty<Link>());

            var links = new List<Link> { _linkBuilder.BuildSelfLink(item.Id,"self") };          

            return new Links(links);
        }

        public IDictionary<string, Relationship> BuildResourceRelationships(Episode item)
        {
            return new Dictionary<string, Relationship>();
        }
    }
}
