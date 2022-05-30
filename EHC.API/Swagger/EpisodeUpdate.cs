using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class EpisodeUpdateParameterMapping 
    {
        public string Path => "/v2/episodes/{id}";

        public HttpMethod Method => HttpMethod.Put;

        public string ParameterName => "episode";

        public string ModelName => "EpisodeUpdate";

        OpenApiSchema Schema => CreateSchema();

        private OpenApiSchema CreateSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Example = new OpenApiString(EmbeddedFiles.EpisodeUpdate)
            };
        }
    }

}
