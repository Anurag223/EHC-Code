using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Serilog;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Services;

namespace TLM.EHC.Common.Clients.EquipmentApi
{
    public partial interface IEquipmentApiClient
    {
        Task<Equipment> GetEquipmentByWkeId(string wkeid);
    }


    public partial class EquipmentApiClient
    {
        private readonly ISecurityTokenProvider _securityTokenProvider;
        private static readonly ILogger Logger = Log.Logger.ForContext<EquipmentApiClient>();

        public EquipmentApiClient(
            EhcApiConfig config, 
            ISecurityTokenProvider securityTokenProvider, 
            IHttpClientFactory clientFactory
        ){
            _baseUrl = config.EquipmentApi.BaseUrl;
            _securityTokenProvider = securityTokenProvider;
            _httpClient = clientFactory.CreateClient();

            if (!string.IsNullOrWhiteSpace(config.EquipmentApi.XApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-apikey", config.EquipmentApi.XApiKey);
            }

            _settings = new Lazy<JsonSerializerSettings>(() =>
            {
                var settings = new JsonSerializerSettings();
                return settings;
            });
        }


        public async Task<Equipment> GetEquipmentByWkeId(string wkeid)
        {
            Logger.Information("GetEquipmentByWkeId: " + wkeid);

            string token = await _securityTokenProvider.GetTokenForEquipmentApi();

            if (token != null)
            {
                _httpClient.SetBearerToken(token);
            }

            // because 'EquipmentCollection' is not correct as for now
            //            string include = null;
            //            EquipmentCollection collection = await this.GetAsync(null, null, null, null, null, null, null, null, null, null, null, null, null, null, include);


            string url = _baseUrl + "/equipment?include=classifications&filter[wellKnownEntityId]=" + wkeid;

            var response = await _httpClient.GetAsync(new Uri(url));

            if (!response.IsSuccessStatusCode)
            {
                throw new ServerErrorException("Request to EquipmentAPI failed. " + response.ReasonPhrase);
            }

            string content = await response.Content.ReadAsStringAsync();
            var collection = JsonConvert.DeserializeObject<EquipmentCollectionFixed>(content, JsonSerializerSettings);

            if (collection.Collection.Count == 0)
            {
                return null;
            }

            if (collection.Collection.Count > 1)
            {
                throw new ServerErrorException("More than 1 equipment from EquipmentAPI returned: " + wkeid);
            }

            return collection.Collection.First();
        }



        private class EquipmentCollectionFixed
        {
            [JsonProperty("collection", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
            public ICollection<Equipment> Collection { get; set; }
        }

    }



}
