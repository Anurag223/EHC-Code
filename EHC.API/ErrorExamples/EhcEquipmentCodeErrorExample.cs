using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ErrorExamples
{
    public class EhcEquipmentCodeErrorExample
    {
        public static Error EquipmentCodeNotFound(string key, string equipmentCode)
        {
            return
                new Error((string) null, (object) 404, (Link) null, (string) null,
                    "Equipment code not found",
                    String.Format(EhcConstants.EquipmentCodeNotFound, equipmentCode), new ErrorSource((string)null, key),
                    (IDictionary<string, object>)null);

        }
    }
}
