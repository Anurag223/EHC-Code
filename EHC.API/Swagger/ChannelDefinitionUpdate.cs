using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class ChannelDefinitionUpdateParameterMapping 
    {
        public string Path => "/v2/channel-definitions/{code}";

        public HttpMethod Method => HttpMethod.Put;

        public string ParameterName => "channelDefinition";

        public string ModelName => "ChannelDefinitionCreate";

        OpenApiSchema Schema => CreateSchema();

        private OpenApiSchema CreateSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Example = new OpenApiString(EmbeddedFiles.ChannelDefinitionUpdate)
            };
        }
    }
}
