using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.HyperLinks
{
    public interface IHyperLinksProvider
    {
        HyperLinkDictionary GetHyperLinks(RowsRequest rowsRequest, TimePeriod timePeriod);
    }
}
