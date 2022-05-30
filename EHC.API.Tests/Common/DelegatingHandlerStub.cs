using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EHC.API.Tests.Common
{
    /// <summary>
    /// This is a helper class for constructing custom message handlers when configuring HttpClient
    /// mocks. Refer to ChannelControllerIntegrationTests::ConfigureStandardHttpClientFactoryMock
    /// method for an example of hwo this can be used.
    /// </summary>
    class DelegatingHandlerStub : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handlerFunc;

        /// <summary>
        /// Use this constructor to configure httpClient to return OK response by default.
        /// </summary>
        public DelegatingHandlerStub()
        {
            _handlerFunc = (request, cancellationToken) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }

        /// <summary>
        /// Use this constructor to configure a custom handler that will be executed when
        /// relevant get/post method is called on httpClient object.
        /// </summary>
        /// <param name="handlerFunc"></param>
        public DelegatingHandlerStub(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
        {
            _handlerFunc = handlerFunc;
        }

        /// <summary>
        /// Use this method to shadow the HttpClient.SendAsync method.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handlerFunc(request, cancellationToken);
        }
    }
}
