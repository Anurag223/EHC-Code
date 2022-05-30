using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.Common.Historian
{
    /// <summary>
    /// Takes query text and returns deserialized result
    /// </summary>
    public interface IHistorianClient
    {
        Task<QueryResult> PerformQuery(Query query);
        Task<InfluxResponse> PerformMultiQuery(Query query);
        Task<DateTime?> GetLatestTimestamp(Query queryLatestTimestamp);
        Task<QueryResult> ShowDatabases();
        Task<QueryResult> CreateDatabase(string dbName);
        Task<InfluxResponse> PerformMultiQuery(string url);

    }

    [ExcludeFromCodeCoverage]
    public class QueryResult
    {
        public string Name { get; set; }
        public List<string> Columns { get; set; } // tags + fields
        public List<List<object>> Values { get; set; }
        public EquipmentInstanceResultSet Tags { get; set; } 
    }

    [ExcludeFromCodeCoverage]
    public class EquipmentInstanceResultSet
    {
        public string EquipmentInstance { get; set; }

    }

    // currently we don't use library "Vibrant.InfluxDB.Client" to query data
    // because it gives overhead by sending extra query to detect tags vs fields
    // that library is used only for writing the data

    public class HistorianClient : IHistorianClient
    {
        private readonly HttpClient _client;
        private readonly IUrlBuilder _urlBuilder;


        public HistorianClient(IHttpClientFactory clientFactory, IUrlBuilder urlBuilder, EhcApiConfig config)
        {
            _client = clientFactory.CreateClient();
            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(MediaTypeWithQualityHeaderValue.Parse("application/json"));

            if (!string.IsNullOrWhiteSpace(config.InfluxDB.Username))
            {
                _client.SetBasicAuthentication(config.InfluxDB.Username, config.InfluxDB.Password);
            }

            _urlBuilder = urlBuilder;
        }


        public async Task<QueryResult> PerformQuery(Query query)
        {
            var influxResponse = await PerformMultiQuery(query);
            var queryResult = ExtractQueryResult(influxResponse);

            RemoveColumn("EquipmentCode", queryResult);
            RemoveColumn("EquipmentInstance", queryResult);
            RemoveColumn("EquipmentSerialNumber", queryResult);
            RemoveColumn("Episode_1", queryResult);

            return queryResult;
        }

        private QueryResult ExtractQueryResult(InfluxResponse influxResponse)
        {
            if (influxResponse == null)
            {
                return null;
            }

            if (influxResponse.Results.Length == 0 || influxResponse.Results[0].Series == null || influxResponse.Results[0].Series.Length == 0)
            {
                return null; // empty response collection
            }

            if (influxResponse.Results.Length > 1 || influxResponse.Results[0].Series.Length > 1)
            {
                throw new EhcApiException("More than one data series returned.");
            }

            var queryResult = influxResponse.Results[0].Series[0];
            return queryResult;
        }

        public async Task<InfluxResponse> PerformMultiQuery(Query query)
        {
            string url = _urlBuilder.GetQueryUrl(query);
            return await PerformMultiQuery(url);
        }


        public async Task<InfluxResponse> PerformMultiQuery(string url)
        {
            var response = await _client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                string errorMessage = "InfluxDB HTTP request failed. " + response.ReasonPhrase;
                if(response.ReasonPhrase.ToLower().Contains("bad request"))
                {
                    throw new BadRequestException(errorMessage);
                }
                throw new EhcApiException(errorMessage);
            }

            string content = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<InfluxResponse>(content);

            return result;
        }


        public async Task<QueryResult> ShowDatabases()
        {
            string url = _urlBuilder.GetUrlShowDatabases();
            var influxResponse = await PerformMultiQuery(url);
            var queryResult = ExtractQueryResult(influxResponse);
            return queryResult;
        }
        public async Task<QueryResult> CreateDatabase(string dbName)
        {
            string url = _urlBuilder.GetUrlCreateDatabase(dbName);
            var influxResponse = await PerformMultiQuery(url);
            var queryResult = ExtractQueryResult(influxResponse);
            return queryResult;
        }
        private void RemoveColumn(string column, QueryResult queryResult)
        {
            if (queryResult?.Columns == null || queryResult.Columns.Count == 0)
            {
                return;
            }

            int index = queryResult.Columns.IndexOf(column);

            if (index < 0)
            {
                return;
            }

            queryResult.Columns.RemoveAt(index);

            foreach (var row in queryResult.Values)
            {
                row.RemoveAt(index);
            }
        }


        // https://community.influxdata.com/t/how-to-get-latest-24h-period-with-data-using-only-one-query/11391

        public async Task<DateTime?> GetLatestTimestamp(Query queryLatest)
        {
            var queryResult = await PerformQuery(queryLatest);

            if (queryResult == null)
            {
                return null;
            }

            if (queryResult.Values.Count > 1)
            {
                throw new EhcApiException("Invalid query: more than one value returned.");
            }

            var timestamp = (DateTime)queryResult.Values[0][0];
            return timestamp;
        }

    }


    [ExcludeFromCodeCoverage]
    public class InfluxSeries
    {
        public QueryResult[] Series { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class InfluxResponse
    {
        public InfluxSeries[] Results { get; set; }
    }
}
