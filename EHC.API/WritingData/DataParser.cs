using MoreLinq;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using TLM.EHC.API.Common;
using TLM.EHC.API.ControllerModels;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.WritingData
{
    public class DataParser : IDataParser
    {
        public ParsedChannels ParseChannels(JToken inputJson)
        {
            var rootObject = inputJson as JObject;

            if (rootObject == null)
            {
                throw new BadRequestException("JSON object expected.");
            }

            JToken currentToken = rootObject;

            if (rootObject["meta"] != null)
            {
                currentToken = rootObject["meta"];
            }

            JArray channelArray = currentToken["channels"] as JArray;

            if (channelArray == null)
            {
                throw new BadRequestException("No 'channels' property with array found.");
            }

            if (channelArray.Count == 0)
            {
                throw new BadRequestException("Empty 'channels' array");
            }

            var parsedChannels = new ParsedChannels();
            parsedChannels.Channels = channelArray.ToObject<ChannelIndexed[]>();

            if (parsedChannels.Channels.First().Index.HasValue)
            {
                parsedChannels.DataFormat = DataFormat.Explicit;
            }
            else
            {
                parsedChannels.DataFormat = DataFormat.Implicit;
            }

            return parsedChannels;
        }

        public ParsedChannels ParseChannelsData(JToken inputJson)
        {
            JArray channelArray = inputJson as JArray;

            if (channelArray == null)
            {
                throw new BadRequestException("No 'channels' property with array found.");
            }

            if (channelArray.Count == 0)
            {
                throw new BadRequestException("Empty 'channels' array");
            }

            var parsedChannels = new ParsedChannels();
            parsedChannels.Channels = channelArray.ToObject<ChannelIndexed[]>();

            if (parsedChannels.Channels.First().Index.HasValue)
            {
                parsedChannels.DataFormat = DataFormat.Explicit;
            }
            else
            {
                parsedChannels.DataFormat = DataFormat.Implicit;
            }

            return parsedChannels;
        }

        public ParsedRows ParseRows(JToken inputJson)
        {
            var rootObject = inputJson as JObject;

            if (rootObject == null)
            {
                throw new BadRequestException("JSON object expected.");
            }

            JArray rowsArray = rootObject["rows"] as JArray;

            if (rowsArray == null)
            {
                throw new BadRequestException("No 'rows' property with array found.");
            }

            if (rowsArray.Count == 0)
            {
                throw new BadRequestException("Empty 'rows' array");
            }

            var parsedRows = new ParsedRows();

            var firstRow = rowsArray.First;
            var firstItem = firstRow.First;

            if (firstItem is JArray)
            {
                parsedRows.DataFormat = DataFormat.Explicit;
                parsedRows.Rows = ParseRows_Explicit(rowsArray);
            }
            else if (firstItem is JValue)
            {
                parsedRows.DataFormat = DataFormat.Implicit;
                parsedRows.Rows = ParseRows_Implicit(rowsArray);
            }
            else
            {
                throw new BadRequestException("Unexpected element type: " + firstItem);
            }

            return parsedRows;
        }

        public ParsedRows ParseRowsData(JArray rowsArray)
        {
            if (rowsArray == null)
            {
                throw new BadRequestException("No 'rows' property with array found.");
            }

            if (rowsArray.Count == 0)
            {
                throw new BadRequestException("Empty 'rows' array");
            }

            var parsedRows = new ParsedRows();

            var firstRow = rowsArray.First;
            var firstItem = firstRow.First;

            if (firstItem is JArray)
            {
                parsedRows.DataFormat = DataFormat.Explicit;
                parsedRows.Rows = ParseRows_Explicit(rowsArray);
            }
            else if (firstItem is JValue)
            {
                parsedRows.DataFormat = DataFormat.Implicit;
                parsedRows.Rows = ParseRows_Implicit(rowsArray);
            }
            else
            {
                throw new BadRequestException("Unexpected element type: " + firstItem);
            }

            return parsedRows;
        }

        public ParsedRow[] ParseRows_Explicit(JArray rowsArray)
        {
            var rows = rowsArray.ToObject<object[][][]>();

            if (rows.Length == 0)
            {
                throw new BadRequestException("At least one row should be sent.");
            }

            var parsedRows = new ParsedRow[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                parsedRows[i] = ParseRow_Explicit(rows[i]);
            }

            return parsedRows;
        }

        private ParsedRow ParseRow_Explicit(object[][] row)
        {
            /*
                [0, "2019-07-29T15:30:04.431Z"],
                [1, 655830.84],
                [2, 491541.375],
                [3, 0.0870783925056458]
            */

            if (row.Length < 2)
            {
                throw new BadRequestException("Timestamp and at least one value should present in each row.");
            }

            var parsedRow = new ParsedRow();
            parsedRow.Values = new ParsedValue[row.Length];

            for (int i = 0; i < row.Length; i++)
            {
                var point = row[i];

                if (point.Length != 2)
                {
                    throw new BadRequestException("Data point array should have exactly 2 items: index and value");
                }

                var parsedValue = new ParsedValue
                {
                    Index = Convert.ToInt32(point[0]),
                    Value = point[1]
                };

                parsedRow.Values[i] = parsedValue;
            }

            return parsedRow;
        }

        public ParsedRow[] ParseRows_Implicit(JArray rowsArray)
        {
            var rows = rowsArray.ToObject<object[][]>();

            if (rows.Length == 0)
            {
                throw new BadRequestException("At least one row should be sent.");
            }

            var parsedRows = new ParsedRow[rows.Length];

            for (int i = 0; i < rows.Length; i++)
            {
                parsedRows[i] = ParseRow_Implicit(rows[i]);
            }

            return parsedRows;
        }

        private ParsedRow ParseRow_Implicit(object[] row)
        {
            /*
                [
                    "2019-07-29T15:30:04.431Z",
                    655830.84,
                    491541.375,
                    0.0870783925056458
                ]
             */

            var parsedRow = new ParsedRow();
            parsedRow.Values = new ParsedValue[row.Length];

            for (int i = 0; i < row.Length; i++)
            {
                var value = row[i];

                var parsedValue = new ParsedValue
                {
                    Index = null,
                    Value = value
                };

                parsedRow.Values[i] = parsedValue;
            }

            return parsedRow;
        }

        public ParsedRows[] ParseRowsBulk(JToken inputJson)
        {
            var dataArray = inputJson["data"] as JArray;
            if (dataArray == null)
            {
                throw new BadRequestException("No 'data' property found.");
            }

            var list = new List<ParsedRows>();

            foreach (JObject jObject in dataArray)
            {
                string equipmentWkeId = jObject["equipmentWkeId"].Value<string>();

                if (string.IsNullOrWhiteSpace(equipmentWkeId))
                {
                    throw new BadRequestException("No 'equipmentWkeId' property with value found.");
                }

                var parsedRows = ParseRows(jObject);
                if (equipmentWkeId.Contains(","))
                {
                    //We are dealing with the ALL case, split the multiple WKEIDs and add
                    //the same row multiple times. We also need an independent rows object
                    //for each WKEID.
                    string[] wkeIdList = equipmentWkeId.Split(',');
                    for (int count = 0; count < wkeIdList.Length; count++)
                    {
                        var rowset = new ParsedRows(parsedRows);
                        rowset.EquipmentWkeId = WellKnownEntityId.Parse(wkeIdList[count]);
                        list.Add(rowset);
                    }
                }
                else
                {
                    parsedRows.EquipmentWkeId = WellKnownEntityId.Parse(equipmentWkeId);
                    //Add new set of rows if we do not have an existing element of parsed
                    //rows for this wkeid.
                    ParsedRows sharedRow = list.Where(p => p.EquipmentWkeId.Value == parsedRows.EquipmentWkeId.Value).FirstOrDefault();
                    if (sharedRow != null)
                    {
                        //Merge current rowset into sharedrow - no need to add new element to our list.
                        ParsedRow[] rowdata = sharedRow.Rows.Concat(parsedRows.Rows).ToArray();
                        sharedRow.Rows = rowdata;
                    }
                    else
                    {
                        list.Add(parsedRows);
                    }                   
                }
            }

            return list.ToArray();
        }

        public ParsedRows[] ParseRowsBulkData(JToken inputJson)
        {
            var dataArray = inputJson as JArray;
            if (dataArray == null)
            {
                throw new BadRequestException("No 'data' property found.");
            }

            var list = new List<ParsedRows>();

            foreach (var jObject in dataArray.Children())
            {
                var objectPpty = jObject.Children<JProperty>();
                string equipmentWkeId =objectPpty.FirstOrDefault(x => x.Name == "EquipmentWkeId").Value.ToString();               

                if (string.IsNullOrWhiteSpace(equipmentWkeId))
                {
                    throw new BadRequestException("No 'equipmentWkeId' property with value found.");
                }
                JArray rowsArray = jObject["Rows"] as JArray;
                var parsedRows = ParseRowsData(rowsArray);
                if (equipmentWkeId.Contains(","))
                {
                    //We are dealing with the ALL case, split the multiple WKEIDs and add
                    //the same row multiple times.
                    string[] wkeIdList = equipmentWkeId.Split(',');
                    for (int count = 0; count < wkeIdList.Length; count++)
                    {
                        parsedRows.EquipmentWkeId = WellKnownEntityId.Parse(wkeIdList[count]);
                        list.Add(parsedRows);
                    }
                }
                else
                {
                    parsedRows.EquipmentWkeId = WellKnownEntityId.Parse(equipmentWkeId);
                    list.Add(parsedRows);
                }
                
            }

            return list.ToArray();
        }

        public string ParseEpisodeId(JToken inputJson)
        {
            return inputJson["meta"]?["episodeId"]?.Value<string>();
        }

        // it just converts structure of SendBulkWithMappings.json into SendBulk.json
        public JToken ConvertEquipmentMappingsToStandardBulk(JToken jToken)
        {
            try
            {
                JArray mappings = jToken["equipmentWkeMappings"] as JArray;
                var deviceIdToWokeIdDictionary = new Dictionary<string, string>();

                foreach (JObject mapping in mappings)
                {
                    deviceIdToWokeIdDictionary.Add(mapping["deviceId"].Value<string>(), mapping["equipmentWkeId"].Value<string>());
                }

                JObject body = jToken["body"] as JObject;
                JArray data = body["data"] as JArray;

                foreach (JObject device in data)
                {
                    string deviceId = device["deviceId"].Value<string>();
                    if (deviceId.ToUpper().CompareTo("ALL") == 0)
                    {
                        string allDeviceIds = string.Join(",",
                            deviceIdToWokeIdDictionary.OrderBy(d => d.Key).Select(d => d.Value));
                        device.Add("equipmentWkeId", allDeviceIds);
                       
                    }
                    else
                    {
                        string equipmentWkeId = deviceIdToWokeIdDictionary[deviceId];
                        device.Add("equipmentWkeId", equipmentWkeId);
                    }
                }

                return body;
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Error parsing equipmentWkeMappings.", ex);
            }
        }

        //TODO: To be checked for deletion
        public JToken ConvertEquipmentMappingsToStandardBulkData(BulkRowsRequest bulkData)
        {
            try
            {
                var dic = new Dictionary<string, string>();
                foreach (var mapping in bulkData.EquipmentWkeMappings)
                {
                    dic.Add(mapping.DeviceId, mapping.EquipmentWkeId);
                }
               
                var body = AddEquipmentWkeIdToData(dic, bulkData);

                return body;
            }
            catch (Exception ex)
            {
                throw new BadRequestException("Error parsing equipmentWkeMappings.", ex);
            }
        }

        //TODO: To be checked for deletion
        private static JObject AddEquipmentWkeIdToData(Dictionary<string, string> dic, BulkRowsRequest bulkData)
        {
            foreach (var data in bulkData.Body.Data)
            {
                string deviceId = data.DeviceId;
                string equipmentWkeId = dic[deviceId];
                data.EquipmentWkeId = equipmentWkeId;
            }
            JObject body = JObject.FromObject(bulkData.Body);

            return body;

        }
    }
}
