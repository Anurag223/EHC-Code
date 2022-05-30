using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NSubstitute;
using NSubstitute.Core;
using Tlm.Sdk.Core.Data;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Services;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.API.ControllerModels.Separated;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.Services
{
    [UnitTestCategory]
    [TestClass]
    public class EpisodeServiceTest
    {
        private Mock<IEpisodeService> _mockEpisodeService;
        private MockRepository _mockProvider;
        private Mock<IRepositoryHandler<Episode>> _mockRepositoryHandler;

        [TestInitialize]

        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockEpisodeService = _mockProvider.Create<IEpisodeService>();
            _mockRepositoryHandler = _mockProvider.Create<IRepositoryHandler<Episode>>();
        }


        protected EpisodeService GetEpisodeService()
        {
            return new EpisodeService(_mockRepositoryHandler.Object);
        }
    }
}
