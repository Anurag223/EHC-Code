using System;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Core.Models.Hypermedia;

// ReSharper disable once CheckNamespace
namespace TLM.EHC.API.Common.ErrorExamples
{
    public static class EhcEquipmentIdErrorExample
    {
        public static Error InvalidEquipmentWkeid(string key, string id) => Error.InvalidParameter(key,
            "Invalid EquipmentWkeId", String.Format(EhcConstants.InvalidEquipmentWkeid, id));

        public static Error EquipmentNotFound(string key, string code)
        {
            return
                new Error(null, 404, null, null,
                    "Equipment not found", String.Format(EhcConstants.EquipmentNotFound, code),
                    new ErrorSource(null, key));
        }
    }
}
