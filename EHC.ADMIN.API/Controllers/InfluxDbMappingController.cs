using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Querying;
using TLM.EHC.Admin;
using TLM.EHC.ADMIN.API.ControllerModels;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;

namespace TLM.EHC.ADMIN.API.Controllers
{
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("/admin/v2")]
    [RootPolicy]
    public class InfluxDbMappingController : CoreController
    {
        private readonly IAdminApiImplementation _adminApiImplementation;

        public InfluxDbMappingController(IAdminApiImplementation adminApiImplementation)
        {
            _adminApiImplementation = adminApiImplementation;
        }

        /// <summary>
        /// Get all influxdb mapping data 
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns>The all data for InfluxDbMapping</returns>
        [HttpGet("influxdbmappings")]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<InfluxDBMapping>))]
        [ProducesNoContentResponseType]
        [ProducesJson]
        public async Task<IActionResult> GetInfluxDbMapping(
            [QuerySpecBinder(typeof(InfluxDBMapping), ExclusionPolicies = QueryExclusionPolicies.None,
                Key = nameof(InfluxDbMappingController))]
            QuerySpec querySpec)
        {
            try
            {
                var result = await _adminApiImplementation.GetAllInfluxDBMappingData(querySpec);
                if (result != null)
                {
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Get db map details
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns></returns>
        [HttpGet("dbmapdetails")]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<DbMapResponse>))]
        [ProducesNoContentResponseType]
        [ProducesJson]
        public async Task<IActionResult> GetDbMapDetails(
            [QuerySpecBinder(typeof(DbMapResponse), ExclusionPolicies = QueryExclusionPolicies.None,
                Key = nameof(InfluxDbMappingController))]
            QuerySpec querySpec)
        {
            try
            {
                var collectionDetails = await _adminApiImplementation.GetAllInfluxDBMappingData(querySpec);
                if (collectionDetails != null)
                {
                    var conflictStatus = await _adminApiImplementation.GetConflictStatusByEquipmentCode(collectionDetails);
                    var result = FlattenDataByEquipmentCode(collectionDetails, conflictStatus);
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        private List<DbMapResponse> FlattenDataByEquipmentCode(CollectionResult<InfluxDBMapping> collectionResult,
            Dictionary<string, DBMapConflictStatus> conflictStatus)
        {

            var dbResponseList = collectionResult.Collection.SelectMany(o => o.EquipmentCodes.Select(
                eqcode => new DbMapResponse()
                {
                    EquipmentCode = eqcode,
                    DbName = o.DbName,
                    MeasurementName = o.MeasurementName,
                    Status = o.Status,
                    ConflictStatus = conflictStatus.Single(x => x.Key == eqcode).Value
                }));

            return dbResponseList.ToList();
        }


        /// <summary>
        /// Update influxdb mapping status and create db in influx when status is set as enabled(true)
        /// </summary>
        /// <param name="equipmentCode"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        [HttpPost("influxdbmappingstatus")]
        [ProducesOkResponseType(typeof(InfluxAndDbMappingUpdateResponse))]
        [ProducesBadRequestResponseType]
        [ProducesNotFoundResponseType]
        [ProducesResponseType(typeof(InfluxAndDbMappingUpdateResponse), (int)HttpStatusCode.InternalServerError)]
        [ProducesJson]
        public async Task<IActionResult> UpdateInfluxDBMappingStatus(string equipmentCode, bool status)
        {

            InfluxAndDbMappingUpdateResponse response = new InfluxAndDbMappingUpdateResponse();
            bool result = false;
            try
            {
                if (string.IsNullOrEmpty(equipmentCode))
                {
                    throw new BadRequestException(EhcConstants.EquipmentCodeCannotBeNullOrEmpty);
                }               
                if (status)
                {
                    await _adminApiImplementation.CreateDbInInflux(equipmentCode);
                    response.InfluxDbCreationMessage = EhcConstants.InfluxDbCreationSuccessMessage;
                }
                result = await _adminApiImplementation.SetInfluxDbMappingStatus(equipmentCode, status);
                response.DbMapUpdateStatus = result;
                response.DbMapUpdateMessage = EhcConstants.DbMappingUpdatedSuccessMessage;
                return result ? (IActionResult)Ok(response) : BadRequest();
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (BadRequestException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                response.DbMapUpdateStatus = false;
                response.DbMapUpdateMessage = EhcConstants.DbMappingUpdatedErrorMessage;
                response.InfluxDbCreationMessage = EhcConstants.InfluxDbCreationErrorMessage;
                response.ErrorDetails = ex.Message;

                return StatusCode(StatusCodes.Status500InternalServerError, response);
            }
        }


        /// <summary>
        /// Create new influxdb mapping using equipment Code
        /// </summary>
        /// <param name="equipmentCode"></param>
        /// <returns></returns>
        [HttpPost("influxdbmapping")]
        [ProducesOkResponseType(typeof(IActionResult))]
        [ProducesBadRequestResponseType]
        [ProducesNotFoundResponseType]
        public async Task<IActionResult> CreateNewInfluxDBMapping(string equipmentCode)
        {
            try
            {
                if (string.IsNullOrEmpty(equipmentCode))
                {
                    throw new BadRequestException("Equipment code cannot be empty");
                }
                var result = await _adminApiImplementation.CreateUpdateDbMap(equipmentCode);
                if (result.MessageForAdmin == EhcConstants.EquipmentCodeAlreadyExists)
                {
                    throw new Exception(result.MessageForAdmin);
                }
                return Ok(result.MessageForAdmin);
            }
            catch (NotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Get all db maps conflict logs data 
        /// </summary>
        /// <param name="querySpec"></param>
        /// <returns>All data about Epic and db maps conflicts</returns>
        [HttpGet("epicdbmapconflictlog")]
        [ProducesBadRequestResponseType]
        [ProducesOkResponseType(typeof(CollectionResult<EpicDBMapConflictLog>))]
        [ProducesNoContentResponseType]
        [ProducesJson]
        public async Task<IActionResult> GetEpicDBMapConflictLog(
            [QuerySpecBinder(typeof(EpicDBMapConflictLog), ExclusionPolicies = QueryExclusionPolicies.None,
                Key = nameof(InfluxDbMappingController))]
            QuerySpec querySpec)
        {
            try
            {
                var result = await _adminApiImplementation.GetAllEpicDBMapConflictLog(querySpec);
                if (result != null)
                {
                    return Ok(result);
                }
                return NoContent();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }


    }
}
