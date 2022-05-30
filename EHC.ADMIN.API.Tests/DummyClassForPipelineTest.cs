using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;

namespace EHC.ADMIN.API.Tests
{
    [UnitTestCategory]
    [TestClass]
    public class DummyClassForPipelineTest
    {
        [TestMethod]
        public void Verify_Dummy_Test()
        {
            Assert.AreEqual(true, true);
        }
    }
}
