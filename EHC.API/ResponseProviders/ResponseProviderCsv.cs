﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Serilog;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.Controllers;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ResponseProviders
{
    public class ResponseProviderCsv : ResponseProvider
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<ResponseProviderCsv>();

        private readonly HttpClient _client;
        private readonly IUrlBuilder _urlBuilder;


        public ResponseProviderCsv(IHttpClientFactory clientFactory, IUrlBuilder urlBuilder, EhcApiConfig config)
        {
            _client = clientFactory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("text/csv"));

            if (!string.IsNullOrWhiteSpace(config.InfluxDB.Username))
            {
                _client.SetBasicAuthentication(config.InfluxDB.Username, config.InfluxDB.Password);
            }

            _urlBuilder = urlBuilder;
        }

        public override async Task<ApiResponse> GetResponse(Query query, QueryContext context)
        {
            var url = _urlBuilder.GetQueryUrl(query);

            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "HTTP request failed. " + response.Content;
                Logger.Error(errorMessage);
                throw new EhcApiException(errorMessage);
            }

            string content = await response.Content.ReadAsStringAsync();
            return new ApiResponse(content, response.Content.Headers.ContentType.MediaType);
        }

        public override Task<ApiResponse> GetResponseNoData(RowsRequest rowsRequest, QueryContext context)
        {
            return Task.FromResult(new ApiResponse("", "text/csv"));
        }
    }
}
