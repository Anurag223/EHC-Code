using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.API.ErrorExamples;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ErrorExamples
{
    [UnitTestCategory]
    [TestClass]
   public class EhcFillValueErrorExampleTest
    {
        [TestMethod]
        public void Verify_InvalidFillValue_ErrorExample()
        {
            var error = EhcFillValueErrorExample.InvalidFillValue("fillValue", "xyz");
            Assert.AreEqual(error.Code, "INVALIDFILLVALUE");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "Invalid Fill Value");

        }
    }
}
