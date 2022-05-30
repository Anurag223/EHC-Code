using System;
using System.Threading.Tasks;
using Moq;
using TLM.EHC.Common.Historian;

namespace EHC.API.Tests.Mocks
{
    public class MockHistorianClient : Mock<IHistorianClient>
    {
        public MockHistorianClient ConfigureMockForPerformQuery(QueryResult queryResult)
        {
            Setup(o => o.PerformQuery(It.IsAny<Query>())).Returns(Task.FromResult((QueryResult)queryResult));
            return this;
        }

        public MockHistorianClient ConfigureMockForGetChannelTimestamp(string url, InfluxResponse influxResponse)
        {
            if (!string.IsNullOrEmpty(url))
                Setup(o => o.PerformMultiQuery(It.IsAny<string>())).Returns(Task.FromResult(influxResponse));
            return this;
        }

        public MockHistorianClient ConfigureMockForGetLatestTimestamp(DateTime? value)
        {
            Setup(o => o.GetLatestTimestamp(It.IsAny<Query>())).Returns(Task.FromResult(value));
            return this;
        }
    }
}
