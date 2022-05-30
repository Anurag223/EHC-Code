using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.ControllerModels;
using Tlm.Sdk.Api;

namespace TLM.EHC.API.Swagger.RequestBodyExampleProvider
{
    [ExcludeFromCodeCoverage]
    // This is created specifically to handle special case of Post Bulk Channels data which can accept two kinds of JSONs -SendBulk.json,SendBulkWithMappings.json
    public class ChannelsPostBulkExampleProvider: BodyExampleProvider 
    {
        public ChannelsPostBulkExampleProvider()
        {
            var bulkRowRequest = new BulkRowsRequest
            {
                EquipmentWkeMappings =
                    new List<EquipmentWkeMappings>()
                    {
                        new EquipmentWkeMappings() {DeviceId = "string", EquipmentWkeId = "string"}
                    },
                Body = new BulkChannelsBody
                {
                    Data = new List<BulkData>()
                    {
                        new BulkData() {DeviceId = "string", Rows = new List<List<object>>()}
                    },
                    Channels = new List<ChannelRequest>()
                    {
                        new ChannelRequest()
                        {
                            Code = "string", Dimension = "string", Index = 0, Uom = "string"
                        }
                    }
                }
            };

            var jsonString = JToken.FromObject(bulkRowRequest);
            var document = CreateDocumentFor(jsonString);
            AddExample(
                Tlm.Sdk.Core.Models.Hypermedia.Constants.MediaType.ApplicationJson,
                document);
        }
    }
}