using System.Diagnostics.CodeAnalysis;
using TLM.EHC.API.Common.ErrorExamples;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class EquipmentNotFoundExampleProvider : BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcEquipmentIdErrorExample.EquipmentNotFound("EquipmentWkeId", "100949474:SPF74311F1449"), "Provide correct Equipment id");
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
