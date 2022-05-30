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
    public class EhcChannelCodeErrorExampleTest
    {
        [TestMethod]
        public void Verify_InvalidChannelCode_ErrorExample()
        {
            var error = EhcChannelCodeErrorExample.InvalidChannelCode("code", "xyz");
            Assert.AreEqual(error.Code, "INVALIDCHANNELCODE");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Title, "Invalid Channel Code");

        }

        [TestMethod]
        public void Verify_ChannelCodeNotFound_ErrorExample()
        {
            var error = EhcChannelCodeErrorExample.ChannelCodeNotFound("code", "xyz");
            Assert.AreEqual(error.Code, "CHANNELCODENOTFOUND");
            Assert.AreEqual(error.Status, "404");
            Assert.AreEqual(error.StatusCode, 404);
            Assert.AreEqual(error.Title, "ChannelCodeNotFound");

        }
    }
}
