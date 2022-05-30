using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TLM.EHC.Common.Services
{
    public interface ISecurityTokenProvider
    {
        Task<string> GetTokenForEquipmentApi();
    }
}
