using System;
using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.Common;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client;
using Vibrant.InfluxDB.Client.Rows;

namespace TLM.EHC.API.WritingData
{
    public class HistorianWriter : IHistorianWriter
    {
        private readonly string _historianUrl;
        private readonly InfluxWriteOptions _options;
        private readonly InfluxClient _client;

        public HistorianWriter(EhcApiConfig config)
        {
            var uri = new Uri(config.InfluxDB.BaseUrl);
            string username = config.InfluxDB.Username;
            string password = config.InfluxDB.Password;

            _client = new InfluxClient(uri, username, password);

            _options = new InfluxWriteOptions();
            _options.UseGzip = true;
            _options.Precision = TimestampPrecision.Nanosecond; // Nanosecond is default
            _options.RetentionPolicy = "autogen"; // "autogen" means store forever

            // https://slb-it.visualstudio.com/es-TLM-federation/_workitems/edit/1147053
            _options.Consistency = Consistency.One;
        }

        public async Task WriteData(DynamicInfluxRow[] rows, InfluxPath influxPath, string suffix)
        {
      
            string db = influxPath.Technology;
            string measurement = influxPath.Brand + suffix;

            try
            {
                await _client.WriteAsync(db, measurement, rows, _options);
            }
            catch (InfluxException ex) 
            {
                // https://slb-it.visualstudio.com/es-TLM-federation/_workitems/edit/1696453
                throw ExceptionTranslator.GetExceptionType(ex.Message);
            }
        }
    }
}
