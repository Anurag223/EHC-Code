using System.Collections.Generic;

namespace TLM.EHC.API.ControllerModels
{
    /// <summary>
    /// Common parent for all entities to be serialized as JSON response
    /// </summary>
    public abstract class ResponseEntity
    {
        public HyperLinkDictionary Links { get; set; }
    }


    /// <summary>
    /// Hyper Link Dictionary
    /// </summary>
    public class HyperLinkDictionary : Dictionary<string, HyperLink>
    {
    }


    /// <summary>
    /// Hyper Link
    /// </summary>
    public class HyperLink
    {
        public string Href { get; set; }
        public string Rel { get; set; }

        public HyperLink()
        {
        }

        public HyperLink(string rel, string href)
        {
            Rel = rel;
            Href = href;
        }
    }
}
