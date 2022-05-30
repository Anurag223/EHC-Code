using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.ErrorExamples;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    public class EquipmentCodeNotFoundExampleProvider:BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcEquipmentCodeErrorExample.EquipmentCodeNotFound("equipmentcode", "SPF-177"), "Provide correct Equipment code");
            }
        }

        protected void AddErrorExample(Error error, string description = null) =>
            AddExample(
                "application/json",
                error.Title,
                description ?? error.Detail,
                error);
    }
}
