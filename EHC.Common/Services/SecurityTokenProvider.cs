using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Serilog;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.Common.Services
{
    public class SecurityTokenProvider : ISecurityTokenProvider
    {
        private static readonly ILogger Logger = Log.Logger.ForContext<SecurityTokenProvider>();

        private readonly EhcApiConfig _config;
        private readonly ICurrentTime _currentTime;
        private readonly HttpClient _client;

        private string _token;
        private DateTime _tokenTime;

        public SecurityTokenProvider(EhcApiConfig config, ICurrentTime currentTime, IHttpClientFactory clientFactory)
        {
            _config = config;
            _currentTime = currentTime;
            _client = clientFactory.CreateClient();
        }

        public async Task<string> GetTokenForEquipmentApi()
        {
            if (string.IsNullOrWhiteSpace(_config.EquipmentApi.TokenAddress))
            {
                return null;
            }

            if (_token != null && _currentTime.Now.Subtract(_tokenTime).TotalMinutes < _config.EquipmentApi.TokenCachingInMinutes)
            {
                return _token;
            }

            Logger.Information("Requesting new token for Equipment API.");

            _token = await RequestNewToken();
            _tokenTime = _currentTime.Now;

            return _token;
        }


        private async Task<string> RequestNewToken()
        {
            var response = await _client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = _config.EquipmentApi.TokenAddress,
                Scope = _config.EquipmentApi.TokenScope,
                ClientId = _config.EquipmentApi.TokenClientId,
                ClientSecret = _config.EquipmentApi.TokenClientSecret
            });

            if (response.IsError)
            {
                throw new ServerErrorException($"Error while requesting Equipment API token: { response.Error }");
            }

            return response.AccessToken;
        }
    }
}
