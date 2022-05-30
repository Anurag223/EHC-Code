#nullable enable
using System.Diagnostics.CodeAnalysis;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.Controllers
{
    [ExcludeFromCodeCoverage]
    public class RowsRequest
    {
        public DataType DataType { get; set; }
        public QueryType QueryType { get; set; }
#pragma warning disable 8618
        public WellKnownEntityId WKEid { get; set; }
#pragma warning restore 8618
#pragma warning disable 8618
        public string[] Codes { get; set; }
#pragma warning restore 8618
        public TimePeriod TimePeriod { get; set; }
        public string EpisodeId { get; set; }
        public ResponseFormat ResponseFormat { get; set; }
        public AggregationFunctions? AggregateFunction { get; set; }
        public string? GroupbyTimeValue { get; set; }
        public string? FillValue { get; set; }
        public MathFunctions MathFunction { get; set; }

    }
}
