using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class ChannelExplicitParameterMapping
    {
        public string Path => "/v2/equipment/{equipmentwkeid}/channels"; // should be lower-cased!

        public HttpMethod Method => HttpMethod.Post;

        public string ParameterName => "channelData";

        public string ModelName => "ChannelExplicit";

        OpenApiSchema Schema => CreateSchema();

        private OpenApiSchema CreateSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Example = new OpenApiString(EmbeddedFiles.ChannelExplicit)
            };
        }
    }


    // Mateo class 'EdpPatchDocumentDefinitionProvider' was used as a reference

}