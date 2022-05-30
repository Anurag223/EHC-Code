using System.Diagnostics.CodeAnalysis;
using TLM.EHC.Common.Models;

namespace TLM.EHC.Common.Historian
{
    public class Query
    {
        public string Database { get; }
        public string SelectText { get; }

        public Query(string database, string selectText)
        {
            Database = database;
            SelectText = selectText;
        }
    }

    // get rid of this class as only one property left?
    [ExcludeFromCodeCoverage]
    public class QueryContext
    {
        public Equipment Equipment { get; set; }
    }


    public enum QueryType
    {
        Unknown,
        SingleCode,
        MultipleCodes,
        Definitions
    }


    public enum DataType
    {
        Unknown,
        Channel,
        Reading,
        Episodic
    }

    public enum ResponseFormat
    {
        Unknown,
        Default,
        V1,
        V2,
        Influx,
        CSV
    }
}
