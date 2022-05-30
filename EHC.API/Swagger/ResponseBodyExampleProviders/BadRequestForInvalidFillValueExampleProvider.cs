using System.Diagnostics.CodeAnalysis;
using TLM.EHC.API.ErrorExamples;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class BadRequestForInvalidFillValueExampleProvider : BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcFillValueErrorExample.InvalidFillValue("fillValue", "ertete"), "Provide fill value in correct format");

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
