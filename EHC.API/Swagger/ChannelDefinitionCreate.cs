using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class ChannelDefinitionCreateParameterMapping
    {
        public string Path => "/v2/channel-definitions";

        public HttpMethod Method => HttpMethod.Post;

        public string ParameterName => "channelDefinition";

        public string ModelName => "ChannelDefinitionCreate";

        OpenApiSchema Schema => CreateSchema();

        private OpenApiSchema CreateSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Example = new OpenApiString(EmbeddedFiles.ChannelDefinitionCreate)
            };
        }
    }
}
