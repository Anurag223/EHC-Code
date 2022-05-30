using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ResponseProviders
{
    public class ResponseProviderSingleChannel : ResponseProviderV2
    {
        private readonly IHistorianClient _historianClient;
        private readonly IChannelDefinitionService _channelDefinitionService;

        public ResponseProviderSingleChannel(IHistorianClient historianClient, IChannelDefinitionService channelDefinitionService)
        {
            _historianClient = historianClient;
            _channelDefinitionService = channelDefinitionService;
        }


        public override async Task<ApiResponse> GetResponse(Query query, QueryContext context)
        {
            SingleChannel result = new SingleChannel();

            result.EquipmentWkeId = context.Equipment.EquipmentWkeId;
            result.MaterialNumber = context.Equipment.MaterialNumber;
            result.SerialNumber = context.Equipment.SerialNumber;
            result.EquipmentCode = context.Equipment.EquipmentCode;
            result.Rows = new List<List<object>>();

            var queryResult = await _historianClient.PerformQuery(query);

            if (queryResult != null)
            {
                result.Channel = await _channelDefinitionService.GetChannelDescription(queryResult.Columns.Last());
                result.Rows = queryResult.Values;
                result.Period = GetResultPeriod(result.Rows);
            }
            else
            {
                // /v2/equipment/{wkeid}/channels/{channelCode}
                // make additional request and return 404 if no any data?
            }

            return new ApiResponse(result);
        }


        public override Task<ApiResponse> GetResponseNoData(RowsRequest rowsRequest, QueryContext context)
        {
            throw new NotFoundException("Channel code not found: " + rowsRequest.Codes[0]) { ErrorCode = ErrorCodes.ChannelCodeNotFound };
        }


    }
}
