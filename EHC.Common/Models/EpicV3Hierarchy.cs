using System.Diagnostics.CodeAnalysis;

namespace TLM.EHC.Common.Models
{
    [ExcludeFromCodeCoverage]
    public class EpicV3Hierarchy
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string ParentCode { get; set; }
        public EpicV3ClassificationType Type { get; set; }
        public System.Collections.Generic.ICollection<EpicV3Hierarchy> Children { get; set; }

    }


}
