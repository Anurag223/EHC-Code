using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client.Rows;

namespace TLM.EHC.API.WritingData
{
    public interface IHistorianWriter
    {
        Task WriteData(DynamicInfluxRow[] rows, InfluxPath influxPath, string suffix);
    }
}
