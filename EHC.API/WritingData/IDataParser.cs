using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Linq;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client.Rows;

namespace TLM.EHC.API.WritingData
{
    public interface IDataParser
    {
        ParsedChannels ParseChannels(JToken inputJson);
        ParsedChannels ParseChannelsData(JToken inputJson);
        ParsedRows[] ParseRowsBulkData(JToken inputJson);
        ParsedRows ParseRowsData(JArray inputArray);
        ParsedRows ParseRows(JToken inputJson);
        ParsedRows[] ParseRowsBulk(JToken inputJson);
        string ParseEpisodeId(JToken inputJson); // for episodic points
        JToken ConvertEquipmentMappingsToStandardBulk(JToken jToken);

        JToken ConvertEquipmentMappingsToStandardBulkData(BulkRowsRequest bulkRows);
    }

    [ExcludeFromCodeCoverage]
    public class ParsedBulkData
    {
        public ChannelIndexed[] Channels { get; set; }
        public EquipmentRows[] EquipmentRows { get; set; }
        public string EpisodeId { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class EquipmentRows
    {
        public WellKnownEntityId EquipmentWkeId { get; set; }
        public DynamicInfluxRow[] Rows { get; set; }
    }


    public enum DataFormat
    {
        Unknown,
        Explicit, // explicit is main format
        Implicit  // (April 2020) do we still need implicit format after all? probably we can just have explicit format only
    }


    [ExcludeFromCodeCoverage]
    public class ParsedChannels
    {
        public ChannelIndexed[] Channels { get; set; }
        public DataFormat DataFormat { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Index}: {Code} / {Dimension} / {Uom}")]
    public class ChannelIndexed : ChannelDefinitionClean
    {
        public int? IndexValue { get; set; } // used in explicit format
    }

    [ExcludeFromCodeCoverage]
    public class ParsedRows
    {
        public WellKnownEntityId EquipmentWkeId { get; set; } // filled in case of bulk data
        public ParsedRow[] Rows { get; set; }
        public DataFormat DataFormat { get; set; }

        public ParsedRows()
        {

        }

        public ParsedRows(ParsedRows sourceRows)
        {
            //Intentionally not copying the array, instead assigning reference to same object.
            //This copy constructor is being used only by the multiple-channel scenario at the
            //moment. This scenario deals with different wkeids but same set of rows.
            //If a change is being made to one set of rows for any reason, it should reflect
            //for all other WKEIDs as well.
            this.Rows = sourceRows.Rows;
            this.DataFormat = sourceRows.DataFormat;
            this.EquipmentWkeId = sourceRows.EquipmentWkeId;
        }
    }

    [ExcludeFromCodeCoverage]
    public class ParsedRow
    {
        public ParsedValue[] Values { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DebuggerDisplay("{Index}: {Value}")]
    public class ParsedValue
    {
        public int? Index { get; set; } // used in explicit format
        public object Value { get; set; }
    }
}
