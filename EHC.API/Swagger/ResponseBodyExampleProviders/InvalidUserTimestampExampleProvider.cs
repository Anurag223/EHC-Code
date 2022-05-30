using System.Diagnostics.CodeAnalysis;
using TLM.EHC.API.ErrorExamples;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class InvalidUserTimestampExampleProvider:BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcUserTimestampErrorExample.InvalidThresholdTimestamp("thresholdtimestamp", "2020/06/30"), "Provide correct threshold timestamp");
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

