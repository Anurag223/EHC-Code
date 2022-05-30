using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Models;
using Vibrant.InfluxDB.Client.Rows;

namespace EHC.API.Tests.Mocks
{
    public class MockHistorianWriter : Mock<IHistorianWriter>
    {
        public MockHistorianWriter ConfigureMockForWriteData(DynamicInfluxRow[] influxRows, InfluxPath influxPath, string suffix)
        {
            Setup(o => o.WriteData(It.IsAny<DynamicInfluxRow[]>(), It.IsAny<InfluxPath>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            return this;
        }
    }
}
