using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client.Rows;
using ChannelDefinitionIndex = TLM.EHC.API.ControllerModels.ChannelDefinitionIndex;

namespace TLM.EHC.API.Services
{
    public interface IDataMapper
    {
        Task<ChannelDefinitionIndex[]> ValidateAndMapChannels(ParsedChannels parsedChannels);
        DynamicInfluxRow[] MapToInfluxRows(ParsedRows parsedRows, ChannelDefinitionIndex[] channelDefinitions);
        (WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[] MapToInfluxRowsBulk(ParsedRows[] parsedRowsList, ChannelDefinitionIndex[] channelDefinitions);
    }


    public class DataMapper : IDataMapper
    {
        private readonly IChannelDefinitionService _channelDefinitionService;
        private readonly ITimestampParser _timestampParser;

        public DataMapper(
            IChannelDefinitionService channelDefinitionService,
            ITimestampParser timestampParser
        )
        {
            _channelDefinitionService = channelDefinitionService;
            _timestampParser = timestampParser;
        }


        public async Task<ChannelDefinitionIndex[]> ValidateAndMapChannels(ParsedChannels parsedChannels)
        {
            List<ChannelDefinition> resultantChannelDefs = new List<ChannelDefinition>();
            var resultArray = new ChannelDefinitionIndex[parsedChannels.Channels.Length];

            for (int i = 0; i < parsedChannels.Channels.Length; i++)
            {
                var inputChannel = parsedChannels.Channels[i];
                if (resultantChannelDefs != null && (!resultantChannelDefs.Exists(def => def.Code == inputChannel.Code)))
                {
                    var definition = await _channelDefinitionService.GetChannelDefinition(inputChannel.Code);

                    if (definition == null)
                    {
                        throw new BadRequestException("Channel definition not found for code: " + inputChannel.Code);
                    }

                    if (!string.Equals(inputChannel.Dimension, definition.Dimension, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new BadRequestException($"For code '{inputChannel.Code}' you passed dimension '{inputChannel.Dimension}', but expected is '{definition.Dimension}'");
                    }

                    if (!string.Equals(inputChannel.Uom, definition.Uom, StringComparison.InvariantCultureIgnoreCase))
                    {
                        throw new BadRequestException($"For code '{inputChannel.Code}' you passed UOM '{inputChannel.Uom}', but expected is '{definition.Uom}'");
                    }
                    resultantChannelDefs.Add(definition);
                    resultArray[i] = new ChannelDefinitionIndex(definition, inputChannel.Index);
                }
                else
                {
                    throw new BadRequestException($" you have passed duplicate channel '{inputChannel.Code}' in request.");
                }
            }

            return resultArray;
        }


        public DynamicInfluxRow[] MapToInfluxRows(ParsedRows parsedRows, ChannelDefinitionIndex[] channelDefinitions)
        {
            Dictionary<int, ChannelDefinition> dicChannels = null;

            if (parsedRows.DataFormat == DataFormat.Explicit)
            {
                dicChannels = CreateDicChannels(channelDefinitions); // needed for explicit format
            }

            var influxRows = new DynamicInfluxRow[parsedRows.Rows.Length];

            for (int i = 0; i < influxRows.Length; i++)
            {
                influxRows[i] = MapRow(parsedRows.Rows[i], parsedRows.DataFormat, channelDefinitions, dicChannels);
            }

            return influxRows;
        }

        public (WellKnownEntityId wkeid, DynamicInfluxRow[] rows)[] MapToInfluxRowsBulk(ParsedRows[] parsedRowsList, ChannelDefinitionIndex[] channelDefinitions)
        {
            Dictionary<int, ChannelDefinition> dicChannels = null;

            if (parsedRowsList.First().DataFormat == DataFormat.Explicit)
            {
                dicChannels = CreateDicChannels(channelDefinitions); // needed for explicit format
            }

            var result = new List<(WellKnownEntityId wkeid, DynamicInfluxRow[] rows)>();

            foreach (var parsedRows in parsedRowsList)
            {
                var influxRows = new DynamicInfluxRow[parsedRows.Rows.Length];

                for (int i = 0; i < influxRows.Length; i++)
                {
                    influxRows[i] = MapRow(parsedRows.Rows[i], parsedRows.DataFormat, channelDefinitions, dicChannels);
                }

                result.Add((parsedRows.EquipmentWkeId, influxRows));
            }

            return result.ToArray();
        }


        private DynamicInfluxRow MapRow(ParsedRow parsedRow, DataFormat dataFormat, ChannelDefinitionIndex[] channelDefinitions, Dictionary<int, ChannelDefinition> dicChannels)
        {
            if (dataFormat == DataFormat.Implicit && parsedRow.Values.Length != channelDefinitions.Length)
            {
                throw new BadRequestException("In implicit format each row should be exactly same length as number of channels.");
            }

            var influxRow = new DynamicInfluxRow();

            for (int i = 0; i < parsedRow.Values.Length; i++)
            {
                ParsedValue parsedValue = parsedRow.Values[i];
                ChannelDefinition channel;

                if (dataFormat == DataFormat.Explicit)
                {
                    if (dicChannels.TryGetValue(parsedValue.Index.Value, out ChannelDefinition found))
                    {
                        channel = found;
                    }
                    else
                    {
                        throw new BadRequestException("Channel with following index not found: " + parsedValue.Index.Value);
                    }
                }
                else if (dataFormat == DataFormat.Implicit)
                {
                    channel = channelDefinitions[i].ChannelDefinition;
                }
                else
                {
                    throw new Exception("Unexpected data format: " + dataFormat);
                }

                if (channel.Code.Equals("time", StringComparison.InvariantCultureIgnoreCase))
                {
                    influxRow.Timestamp = _timestampParser.Parse(parsedValue.Value);
                }
                else
                {
                    string fieldName = _channelDefinitionService.GetFieldName(channel);
                    influxRow.Fields.Add(fieldName, parsedValue.Value);
                }
            }

            if (influxRow.Timestamp == null)
            {
                throw new BadRequestException("No 'time' channel found.");
            }

            return influxRow;
        }

        private Dictionary<int, ChannelDefinition> CreateDicChannels(ChannelDefinitionIndex[] channelDefinitions)
        {
            if (channelDefinitions.Length < 2)
            {
                throw new BadRequestException("At least two channels should be specified.");
            }

            var dicChannels = new Dictionary<int, ChannelDefinition>();

            foreach (var channel in channelDefinitions)
            {
                if (dicChannels.ContainsKey(channel.Index.Value))
                {
                    throw new BadRequestException("Duplicated channel index: " + channel.Index);
                }

                dicChannels.Add(channel.Index.Value, channel.ChannelDefinition);
            }

            return dicChannels;
        }

    }



}
