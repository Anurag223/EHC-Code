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
    public class EhcChannelDefinitionErrorExampleTest
    {
        [TestMethod]
        public void Verify_ChannelDefinitionNotFound_ErrorExample()
        {
            var error=EhcChannelDefinitionErrorExample.ChannelDefinitionNotFound("code", "xyz");
            Assert.AreEqual(error.Code, "CHANNELDEFINITIONNOTFOUND");
            Assert.AreEqual(error.Status, "404");
            Assert.AreEqual(error.StatusCode,404);
            Assert.AreEqual(error.Detail, "Channel definition xyz not found");

        }
    }
}
