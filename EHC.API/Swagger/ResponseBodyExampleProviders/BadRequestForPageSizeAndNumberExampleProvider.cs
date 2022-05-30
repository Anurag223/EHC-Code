using System.Diagnostics.CodeAnalysis;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.API.ErrorExamples;

namespace TLM.EHC.API.Swagger.ResponseBodyExampleProviders
{
    [ExcludeFromCodeCoverage]
    public class BadRequestForPageSizeAndNumberExampleProvider : BodyExampleProvider
    {
        protected override void CreateExamples()
        {
            if (!bool.TryParse(Context?.ToString() ?? "true", out var isPaged))
                isPaged = true;

            if (isPaged)
            {
                AddErrorExample(EhcExampleErrors.NegativePageSize("page[size]", -1), "Page sizes must be positive");
                AddErrorExample(EhcExampleErrors.InvalidPageSize("page[size]", "FirstPage"), "Page sizes must be positive integers");
                AddErrorExample(EhcExampleErrors.PageSizeGreaterThanMax("page[size]", 50000, 5000), "Page sizes are limited to avoid excessive resource consumption");
                AddErrorExample(Errors.PageSizeLessThanTwo("page[size]"));
                AddErrorExample(EhcExampleErrors.NegativePageNumber("page[number]", -1), "Page numbers must be positive");
              
            }
          
        }


        protected void AddErrorExample(Error error, string description = null) =>
            AddExample(
                Tlm.Sdk.Core.Models.Hypermedia.Constants.MediaType.ApplicationJson,
                error.Title,
                description ?? error.Detail,
                error);
    }
   

}
