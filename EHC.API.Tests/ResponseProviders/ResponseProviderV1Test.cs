using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.ResponseProviders;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ResponseProviders
{
    [UnitTestCategory]
    [TestClass]
    public class ResponseProviderV1Test
    {
        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Legacy V1 JSON has not been implemented.")]
        public void Verify_GetResponse_ThrowsException()
        {
            ResponseProviderV1 respV1 = new ResponseProviderV1();
            respV1.GetResponse(new Query("test", "test"), new QueryContext());
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "No Data. Legacy V1 JSON has not been implemented.")]
        public void Verify_GetResponseNoData_ThrowsException()
        {
            ResponseProviderV1 respV1 = new ResponseProviderV1();
            respV1.GetResponseNoData(new RowsRequest(), new QueryContext());
        }
    }
}
