using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.ADMIN.API.Controllers
{
    /// <summary>
    /// Controller for A2R Utils Audit log apis
    /// </summary>
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("/admin/v2")]
    [RootPolicy]
    public class A2RUtilsAuditLogController : CoreController
    {
        private readonly IAdminApiImplementation _adminApiImplementation;

        /// <summary>
        /// A2RUtilsAuditLogController constructor
        /// </summary>
        /// <param name="adminAapiImplementation"></param>
        public A2RUtilsAuditLogController(IAdminApiImplementation adminApiImplementation)
        {
            this._adminApiImplementation = adminApiImplementation;
        }

        /// <summary>
        /// Get all logs regarding actions performed on a2r utils application 
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns>All audit logs of a2r utils UI application</returns>
        [HttpGet("a2rutilsauditlog")]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<A2RUtilsAuditLog>))]
        [ProducesNoContentResponseType]
        [ProducesJson]
        public async Task<IActionResult> GetA2RUtilsAuditLog(
            [QuerySpecBinder(typeof(A2RUtilsAuditLog), ExclusionPolicies = QueryExclusionPolicies.None,
                Key = nameof(A2RUtilsAuditLogController))]
            QuerySpec querySpec)
        {
            try
            {
                var result = await _adminApiImplementation.GetA2RUtilsAuditLog(querySpec);
                if (result != null)
                {
                    return Ok(result.Collection.ToList());
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


        /// <summary>
        /// Create new audit log based on action performed on a2r utils application
        /// </summary> 
        /// <param name="log">Specify a2rauditlog in json format.See Example</param>
        /// <returns></returns>
        [HttpPost("a2rutilsauditlogs")]
        [ConsumesApplicationJson]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(A2RUtilsAuditLog))]
        [ProducesResponseType(typeof(IActionResult), (int)HttpStatusCode.InternalServerError)]
        [ProducesJson]
        public async Task<IActionResult> CreateA2RUtilsAuditLog([FromBody] A2RUtilsAuditLog log)
        {
            try
            {
                if (log == null) throw new BadRequestException("Invalid json");
                var result = await _adminApiImplementation.CreateA2RUtilsAuditLog(log);
                return Ok(result);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


    }
}
