using System.Threading.Tasks;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;

namespace TLM.EHC.API.ResponseProviders
{
    public abstract class ResponseProvider
    {
        public abstract Task<ApiResponse> GetResponse(Query query, QueryContext context);
        public abstract Task<ApiResponse> GetResponseNoData(RowsRequest rowsRequest, QueryContext context);
    }
}
