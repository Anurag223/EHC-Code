using System;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using TLM.EHC.API.ControllerModels.Separated;
using TLM.EHC.API.Services;

namespace EHC.API.Tests.Mocks
{
    class MockEpisodeService : Mock<IEpisodeService>
    {
        public MockEpisodeService ConfigureMockForGetEpisodeById(string episodeId, Episode ep)
        {
            if (String.IsNullOrEmpty(episodeId))
                Setup(o => o.GetEpisodeById(It.IsAny<string>())).Returns(Task.FromResult(ep));
            else Setup(o => o.GetEpisodeById(episodeId)).Returns(Task.FromResult(ep));
            return this;
        }
    }
}
