using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ResponseProviders
{
    public class ResponseProviderV1 : ResponseProvider
    {
        public override Task<ApiResponse> GetResponse(Query query, QueryContext context)
        {
            throw new BadRequestException("Legacy V1 JSON has not been implemented.");
        }

        public override Task<ApiResponse> GetResponseNoData(RowsRequest rowsRequest, QueryContext context)
        {
            throw new BadRequestException("No Data. Legacy V1 JSON has not been implemented.");
        }
    }
}
