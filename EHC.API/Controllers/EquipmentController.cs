using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Api;

namespace TLM.EHC.API.Controllers
{
    [ApiVersion("2.0")]
    [ApiExplorerSettings(GroupName = "v2")]
    [Route("v2/equipment/{equipmentWkeId}")]
   [RootPolicy]
    public abstract class EquipmentController : BaseController
    {
        protected EquipmentController(ITimestampParser timestampParser): base(timestampParser)
        {
        }

        protected WellKnownEntityId ParseWellKnownEqId(string equipmentWkeId)
        {
            try
            {
                return WellKnownEntityId.Parse(equipmentWkeId);
            }
            catch (Exception ex)
            {
                throw new BadRequestException(ex.Message){ ErrorCode = ErrorCodes.InvalidEquipmentWkeId};
            }
        }

        protected string[] ParseCodes(string codes)
        {
            if (string.IsNullOrWhiteSpace(codes))
            {
                return new string[0];
            }

            return codes.Split(',', StringSplitOptions.RemoveEmptyEntries);
        }


        protected ResponseFormat ParseResponseFormatFromHeader()
        {
            StringValues header = Request.Headers["accept"];

            if (header.Count != 1)
            {
                throw new BadRequestException("Exactly one 'Accept' http header should be specified.");
            }

            switch (header[0])
            {
                case "application/json":
                    return ResponseFormat.V2;

                case "application/vnd.v1+json":
                    return ResponseFormat.V1;

                case "application/vnd.influx+json":
                    return ResponseFormat.Influx;

                case "text/csv":
                    return ResponseFormat.CSV;
            }

            throw new BadRequestException("Invalid accept header value: " + header[0]);
        }

    }
}
