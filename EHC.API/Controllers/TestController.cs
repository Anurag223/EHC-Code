using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TLM.EHC.Common.Historian;
using Tlm.Sdk.Api;

namespace TLM.EHC.API.Controllers
{
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("v2/test")]
    [RootPolicy]
    [ExcludeFromCodeCoverage]
    internal class TestController : BaseController
    {
        private readonly IHistorianClient _historianClient;

        public TestController(IHistorianClient historianClient): base(null)
        {
            _historianClient = historianClient;
        }


        [HttpGet("Alpha")]
        public async Task<IActionResult> Alpha()
        {
            return Ok("hello there!");
        }
    }
}
