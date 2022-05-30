using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.ErrorExamples;

namespace EHC.API.Tests.ErrorExamplesTest
{
    [UnitTestCategory]
    [TestClass]
    public class EhcEpisodeErrorExampleTest
    {
        [TestMethod]
        public void Verify_EpisodeNotFound_ErrorExample()
        {
            var error = EhcEpisodeErrorExample.EpisodeNotFound("code", "xyz");
            Assert.AreEqual(error.Code, "EPISODENOTFOUND");
            Assert.AreEqual(error.Status, "404");
            Assert.AreEqual(error.StatusCode, 404);
            Assert.AreEqual(error.Detail, "Episode not found xyz");

        }
    }
}
