using System;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using TLM.EHC.Admin;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace EHC.API.Tests.Mocks
{
    public class MockInfluxDbMappingService : Mock<IInfluxDBMappingService>
    {
        public MockInfluxDbMappingService ConfigureMockForGetInfluxDbMapping(string id, bool cacheFlag, InfluxDBMapping mappingToReturn)
        {
            if (String.IsNullOrEmpty(id))
                Setup(o => o.GetInfluxDBMapping(It.IsAny<string>(), cacheFlag).Returns(Task.FromResult(mappingToReturn)));
            else Setup(o => o.GetInfluxDBMapping(id,cacheFlag)).Returns(Task.FromResult(mappingToReturn));
            return this;
        }

        public MockInfluxDbMappingService ConfigureMockForCreateUpdateDbMapping(InfluxDBMapping mapping, InfluxMappingResponse mappingResponse)
        {
            Setup(o => o.CreateUpdateDBMapping(It.IsAny<InfluxDBMapping>())).Returns(Task.FromResult(mappingResponse));
            return this;
        }
    }
}
