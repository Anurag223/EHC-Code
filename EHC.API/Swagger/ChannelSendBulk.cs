using System.Diagnostics.CodeAnalysis;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Net.Http;
using TLM.EHC.API.JSON;

namespace TLM.EHC.API.Swagger
{
    [ExcludeFromCodeCoverage]
    public class ChannelSendBulkParameterMapping
    {
        public string Path => "/v2/channels";

        public HttpMethod Method => HttpMethod.Post;

        public string ParameterName => "bulkChannelData";

        public string ModelName => "ChannelSendBulk";

        OpenApiSchema Schema => CreateSchema();

        private OpenApiSchema CreateSchema()
        {
            return new OpenApiSchema
            {
                Type = "object",
                Example = new OpenApiString(EmbeddedFiles.ChannelSendBulk)
            };
        }
    }
}
