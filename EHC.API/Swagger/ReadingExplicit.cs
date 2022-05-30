using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class ReadingExplicitParameterMapping 
    {
        public string Path => "/v2/equipment/{equipmentwkeid}/readings";

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
}
