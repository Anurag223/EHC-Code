using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Exceptions;

namespace EHC.API.Tests.Mocks
{
    class MockChannelDefinitionService :Mock<IChannelDefinitionService>
    {
        public MockChannelDefinitionService ConfigureMockForGetFieldNameSequence(string uom,string anotherUom)
        {
            SetupSequence(o => o.GetFieldName(It.IsAny<string>())).Returns(Task.FromResult(uom)).Returns(Task.FromResult(anotherUom));
            return this;
        }

        public MockChannelDefinitionService ConfigureMockForValidateChannelCodeThrowsException()
        {
            Setup(o => o.ValidateChannelCode(It.IsAny<List<string>>())).Throws(new NotFoundException(
                    EhcConstants.ChannelDefinitionNotFoundForCode + "dummy")
                { ErrorCode = ErrorCodes.ChannelCodeNotFound });
            return this;
        }

        public MockChannelDefinitionService ConfigureMockForValidateChannelCode()
        {
            Setup(o => o.ValidateChannelCode(It.IsAny<List<string>>())).Returns(Task.FromResult(It.IsAny<Task>()));
            return this;
        }
    }
}
