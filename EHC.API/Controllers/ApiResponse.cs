using System;
using System.Diagnostics.CodeAnalysis;
using TLM.EHC.API.ControllerModels;

namespace TLM.EHC.API.Controllers
{
    /// <summary>
    /// Generic final response result to return from API. Either entity or text content.
    /// </summary>
    public class ApiResponse
    {
        public ResponseEntity Entity { get; }
        public ResponseContent Text { get; }

        public ApiResponse(ResponseEntity entity)
        {
            Entity = entity ?? throw new ArgumentNullException();
        }

        public ApiResponse(string content, string contentTypeHeader)
        {
            if (string.IsNullOrWhiteSpace(contentTypeHeader))
            {
                throw new ArgumentException("Empty contentTypeHeader.");
            }

            Text = new ResponseContent(content, contentTypeHeader);
        }
    }

    [ExcludeFromCodeCoverage]
    /// <summary>
    /// To pass through original Influx response
    /// </summary>
    public class ResponseContent
    {
        public string Content { get;  }
        public string ContentTypeHeader { get; }

        public ResponseContent(string content, string contentTypeHeader)
        {
            Content = content;
            ContentTypeHeader = contentTypeHeader;
        }
    }



}
