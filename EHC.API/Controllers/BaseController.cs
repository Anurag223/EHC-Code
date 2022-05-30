using System;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TLM.EHC.API.WritingData;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Hypermedia;
using System.Net;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using ILogger = Serilog.ILogger;

namespace TLM.EHC.API.Controllers
{
    public class BaseController : CoreController
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<BaseController>();

        private readonly ITimestampParser _timestampParser;

        public BaseController(ITimestampParser timestampParser)
        {
            _timestampParser = timestampParser;
        }

        // it is possible to use custom asp.net middleware instead
        // but we would need to modify StartupForAspNetCore from Tlm.Fed.Framework.Api

        protected IActionResult CreateHttpStatus(HttpStatusException ex)
        {
            if (ex is NotFoundException)
            {
                return NotFound(new Error(
                title: ex.ErrorCode != null ? ex.ErrorCode.ToString() : HttpStatusCode.NotFound.ToString(),
                detail: ex.Message,
                status: (int)HttpStatusCode.NotFound,
                source: new ErrorSource(parameter: ParameterName.GetParameterName(ex.ErrorCode))));
            }
            Logger.Error(ex, ex.Message);
            if (ex.InnerException != null)
            {
                Logger.Error(ex.InnerException, "InnerException.");
            }

            if (ex is BadRequestException)
            {
                return BadRequest(new Error(
                   title: ex.ErrorCode != null ? ex.ErrorCode.ToString() : HttpStatusCode.BadRequest.ToString(),
                   detail: ex.Message,
                   status: (int)HttpStatusCode.BadRequest,
                   source: new ErrorSource(parameter: ParameterName.GetParameterName(ex.ErrorCode))));
            }

            if (ex is ServerErrorException)
            {
                string exceptionText = ex.InnerException?.ToString();
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, exceptionText });
            }

            throw new ArgumentException("Unexpected exception type: " + ex.GetType());
        }


        protected ActionResult ConvertToActionResult(ApiResponse apiResponse)
        {
            if (apiResponse == null)
            {
                throw new ArgumentNullException();
            }

            if (apiResponse.Entity != null)
            {
                return Ok(apiResponse.Entity);
            }

            if (apiResponse.Text != null)
            {
                return Content(apiResponse.Text.Content, apiResponse.Text.ContentTypeHeader);
            }

            throw new ArgumentException();
        }



        protected TimePeriod ParseTimePeriod(string start, string end)
        {
            var validStartDate=false;
            var validEndDate=false;
            try
            {
                if (string.IsNullOrWhiteSpace(start) ^ string.IsNullOrWhiteSpace(end))
                {
                    throw new ArgumentException("Time period Start/End should be both specified or both omitted.");
                }

                if (start == null)
                {
                    return null;
                }

                var dateTimeStart = _timestampParser.Parse(start);
                validStartDate = true;
                var dateTimeEnd = _timestampParser.Parse(end);
                validEndDate = true;


                return new TimePeriod(dateTimeStart, dateTimeEnd);
            }
            catch (Exception ex)
            {
                if(validStartDate && !validEndDate)
                  throw new BadRequestException(ex.Message) { ErrorCode = ErrorCodes.InvalidEndDate };
                throw new BadRequestException(ex.Message) { ErrorCode = ErrorCodes.InvalidStartDate };
            }
        }

        protected bool IsValidZuluTimePeriod(string thresholdTimeStamp)
        {
            
                DateTime parsedDate; 
                if(DateTime.TryParseExact(thresholdTimeStamp ?? DateTime.Now.ToString("s") + "Z", "yyyy-MM-ddTHH:mm:ss.FFFK", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out parsedDate))
                {
                    return true;
                }
                else
                {
                    return false;
                }
        }
    }
}
