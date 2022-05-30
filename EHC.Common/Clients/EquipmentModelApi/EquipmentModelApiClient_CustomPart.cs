using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.Common.Clients.EquipmentModelApi
{
    public partial class EquipmentModelApiClient
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<EquipmentModelApiClient>();

        public EquipmentModelApiClient(EhcApiConfig config, IHttpClientFactory clientFactory)
        {
            if (string.IsNullOrWhiteSpace(config.EquipmentModelApi?.BaseUrl))
            {
                throw new ServerErrorException("Empty 'EquipmentModelApiBaseUrl' setting value.");
            }

            _baseUrl = config.EquipmentModelApi.BaseUrl;
            _httpClient = clientFactory.CreateClient();

            if (!string.IsNullOrWhiteSpace(config.EquipmentModelApi.XApiKey))
            {
                _httpClient.DefaultRequestHeaders.Add("x-apikey", config.EquipmentModelApi.XApiKey);
            }

            _settings = new Lazy<JsonSerializerSettings>(() =>
            {
                var settings = new JsonSerializerSettings();
                UpdateJsonSerializerSettings(settings);
                return settings;
            });
        }
    }
}
