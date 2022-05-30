using System;
using System.Diagnostics.CodeAnalysis;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.API.ErrorExamples;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class BadRequestForDateExampleProvider : BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcExampleErrors.InvalidStartDate("start", new DateTime(2020,2,1)), "Provide Start Date in required format");
                AddErrorExample(EhcExampleErrors.InvalidEndDate("end", new DateTime(2021,02,20)), "Provide End Date in required format");

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

