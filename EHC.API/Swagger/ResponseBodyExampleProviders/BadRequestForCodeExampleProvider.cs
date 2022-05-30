﻿using System.Diagnostics.CodeAnalysis;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.API.ErrorExamples;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class BadRequestForCodeExampleProvider : BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                
                AddErrorExample(EhcChannelCodeErrorExample.InvalidChannelCode("code", "xyz"), "Provide correct Channel Code");
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
