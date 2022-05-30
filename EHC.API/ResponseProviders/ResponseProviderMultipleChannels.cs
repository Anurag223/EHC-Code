using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.ResponseProviders
{
    public class ResponseProviderMultipleChannels : ResponseProviderV2
    {
        private readonly IHistorianClient _historianClient;
        private readonly IChannelDefinitionService _channelDefinitionService;

        public ResponseProviderMultipleChannels(
            IHistorianClient historianClient,
            IChannelDefinitionService channelDefinitionService
        )
        {
            _historianClient = historianClient;
            _channelDefinitionService = channelDefinitionService;
        }


        public override async Task<ApiResponse> GetResponse(Query query, QueryContext context)
        {
            var result = GetEmptyResult(context.Equipment);
            var queryResult = await _historianClient.PerformQuery(query);

            if (queryResult != null)
            {
                result.Rows = queryResult.Values;
                result.Meta.Channels = new List<ChannelDefinitionClean>();

                foreach (string column in queryResult.Columns)
                {
                    result.Meta.Channels.Add(await _channelDefinitionService.GetChannelDescription(column));
                }

                result.Meta.Period = GetResultPeriod(result.Rows);
            }

            return new ApiResponse(result);
        }

        public override Task<ApiResponse> GetResponseNoData(RowsRequest rowsRequest, QueryContext context)
        {
            var result = GetEmptyResult(context.Equipment);
            return Task.FromResult(new ApiResponse(result));
        }


        private MultipleChannels GetEmptyResult(Equipment equipment)
        {
            MultipleChannels result = new MultipleChannels();

            result.Meta = new MultipleChannelsMeta();
            result.Meta.EquipmentWkeId = equipment.EquipmentWkeId;
            result.Meta.MaterialNumber = equipment.MaterialNumber;
            result.Meta.SerialNumber = equipment.SerialNumber;
            result.Meta.EquipmentCode = equipment.EquipmentCode;

            result.Rows = new List<List<object>>();
            result.Meta.Channels = null;
            result.Meta.Period = null;

            return result;
        }

    }
}
