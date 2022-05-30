using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Services;

namespace TLM.EHC.Common.Clients.EpicV3Api
{
    public partial interface IEpicV3ApiClient
    {
        Task<EpicRepresentationV3> GetEpicHierarchyInfoFromCode(string code);

        Task<Tuple<EpicRepresentationV3, EpicRepresentationV3>> GetEpicHierarchyInfoForParent(string code);
    }


    public partial class EpicV3ApiClient
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<EpicV3ApiClient>();

        public EpicV3ApiClient(
            EhcApiConfig config,
            IHttpClientFactory clientFactory
        )
        {
            _baseUrl = config.EpicV3Api.BaseUrl;
            _httpClient = clientFactory.CreateClient();

            if (!string.IsNullOrWhiteSpace(config.EpicV3Api.XApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-apikey", config.EpicV3Api.XApiKey);
            }

            _settings = new Lazy<JsonSerializerSettings>(() =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            });
        }


        public async Task<EpicRepresentationV3> GetEpicHierarchyInfoFromCode(string code)
        {
            Logger.Information("GetEpicHierarchyInfoFromCode: " + code);

            string url = _baseUrl + "/epic?filter[includeChildren]=true&filter[wkId]=" + code;

            var response = await _httpClient.GetAsync(new Uri(url));

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new NotFoundException($"Equipment code {code.Substring(2)} not found in Epic V3 Hierarchy"){ErrorCode = ErrorCodes.EquipmentCodeNotFound };
                }
                throw new ServerErrorException("Request to Epic V3 api failed. " + response.ReasonPhrase);
            }
                
            string content = await response.Content.ReadAsStringAsync();
            var collection = JsonConvert.DeserializeObject<EpicV3CollectionFixed>(content, JsonSerializerSettings);

            if (collection.Collection.Count == 0)
            {
                return null;
            }

            return collection.Collection.First();
        }

        public async Task<Tuple<EpicRepresentationV3, EpicRepresentationV3>> GetEpicHierarchyInfoForParent(string code)
        {
            Logger.Information("GetEpicHierarchyInfoForParent: " + code);

            string url = _baseUrl + "/epic?filter[wkId]=" + code + "&filter[includeParent]=true&filter[parentNodeLevel]=2";

            var response = await _httpClient.GetAsync(new Uri(url));

            if (!response.IsSuccessStatusCode)
            {
                if (!response.IsSuccessStatusCode)
                {
                    if (response.ReasonPhrase == "Not Found")
                        throw new NotFoundException("Invalid equipmentcode or equipmentcode does not exists in epic v3 hierarchy");
                    else
                        throw new ServerErrorException("Request to epic v3 api failed. " + response.ReasonPhrase);
                }
            }

            string content = await response.Content.ReadAsStringAsync();
            var collection = JsonConvert.DeserializeObject<EpicV3CollectionFixed>(content, JsonSerializerSettings);

            if (collection.Collection.Count == 0)
            {
                return null;
            }

            return new Tuple<EpicRepresentationV3, EpicRepresentationV3>(collection.Collection.FirstOrDefault(), collection.Collection.FirstOrDefault().Children.FirstOrDefault(x => x.Type.ToString() == "Brand"));
        }


        private class EpicV3CollectionFixed
        {
            [JsonProperty("collection", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<EpicRepresentationV3> Collection { get; set; }
        }

    }





}
