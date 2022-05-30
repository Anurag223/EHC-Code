using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.ResponseProviders;

namespace EHC.API.Tests.Mocks
{
    class MockResponseProviderResolver : Mock<IResponseProviderResolver>
    {
        public MockResponseProviderResolver ConfigureMockForGetResponseProvider(ResponseFormat responseFormat, QueryType queryType, TLM.EHC.API.ResponseProviders.ResponseProvider provider)
        {
            Setup(o => o.GetResponseProvider(responseFormat, queryType)).Returns(provider);
            return this;
        }
    }
}
