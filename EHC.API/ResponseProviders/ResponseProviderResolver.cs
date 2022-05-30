using System;
using Autofac;
using TLM.EHC.Common.Historian;

namespace TLM.EHC.API.ResponseProviders
{
    public interface IResponseProviderResolver
    {
        ResponseProvider GetResponseProvider(ResponseFormat responseFormat, QueryType queryType);
    }



    public class ResponseProviderResolver : IResponseProviderResolver
    {
        private readonly IComponentContext _context;

        public ResponseProviderResolver(IComponentContext context)
        {
            _context = context;
        }


        public ResponseProvider GetResponseProvider(ResponseFormat responseFormat, QueryType queryType)
        {
            switch (responseFormat)
            {
                case ResponseFormat.Default:
                case ResponseFormat.V2:
                    if (queryType == QueryType.SingleCode)
                    {
                        return _context.Resolve<ResponseProviderSingleChannel>();
                    }
                    else if (queryType == QueryType.MultipleCodes)
                    {
                        return _context.Resolve<ResponseProviderMultipleChannels>();
                    }
                    else
                    {
                        throw new ArgumentException("Unexpected query type: " + queryType);
                    }

                case ResponseFormat.V1:
                    return _context.Resolve<ResponseProviderV1>();

                case ResponseFormat.Influx:
                    return _context.Resolve<ResponseProviderInflux>();

                case ResponseFormat.CSV:
                    return _context.Resolve<ResponseProviderCsv>();

                default:
                    throw new ArgumentException("Invalid response format requested: " + responseFormat);
            }
        }
    }
}
