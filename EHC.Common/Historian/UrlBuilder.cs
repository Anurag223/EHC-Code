using System.Text;
using System.Web;

namespace TLM.EHC.Common.Historian
{
    public class UrlBuilder : IUrlBuilder
    {
        private readonly string _influxDbUrl;

        public UrlBuilder(EhcApiConfig config)
        {
            _influxDbUrl = config.InfluxDB.BaseUrl;
        }

        public string GetQueryUrl(Query query)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_influxDbUrl);
            sb.Append("/query?db=" + query.Database);
//            sb.Append("&pretty=true");           // for debug only
//            sb.Append("&u=jguidry&p=jguidry"); // no login/pass needed now
            sb.Append("&q=" + HttpUtility.UrlEncode(query.SelectText));

            return sb.ToString();
        }

        public string GetUrlShowDatabases()
        {
            return _influxDbUrl + "/query?pretty=true&q=show+databases";
        }

        public string GetUrlCreateDatabase(string dbname)
        {
            return _influxDbUrl + "/query?q=create+database " + dbname;
        }


        public string GetLatestTimeStampThroughQueryUrl(string actualDBName,string dbName,string measurementName,string userProvidedDate)
        {
            return _influxDbUrl + "/query?db=" + actualDBName + "&q=SELECT * FROM " + dbName + ".\"autogen\"." + measurementName + "WHERE TIME" + "<" + userProvidedDate + " GROUP BY \"EquipmentInstance\" ORDER BY DESC LIMIT 1";
        }

        public string GetLatestTimeStampForChannel(string actualDBName, string channelCodewithUnit, string dbName,
            string measurementName, string userProvidedDate)
        {
            var channelCode = @"" + channelCodewithUnit + "";
            var query = new Query(actualDBName,
                "SELECT " + '"' + channelCode + '"' + " FROM " + measurementName + " WHERE TIME" + "<" + userProvidedDate + " GROUP BY \"EquipmentInstance\" ORDER BY DESC LIMIT 1");
            return GetQueryUrl(query);
        }
        
        // more types of URLs will be here
    }


    public interface IUrlBuilder
    {
        string GetQueryUrl(Query query);
        string GetUrlShowDatabases();

        string GetLatestTimeStampThroughQueryUrl(string actualDBName, string dbName, string measurementName, string userProvidedDate);

        string GetLatestTimeStampForChannel(string actualDBName, string channelCodewithUnit, string dbName,
            string measurementName, string userProvidedDate);
        string GetUrlCreateDatabase(string dbname);

    }
}
