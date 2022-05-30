using System.Threading.Tasks;
using Moq;
using TLM.EHC.Common.Clients.EquipmentApi;

namespace EHC.API.Tests.Mocks
{
    public class MockEquipmentApiClientService : Mock<IEquipmentApiClient>
    {
        private int _callTracker;

        public MockEquipmentApiClientService ConfigureMockForGetEquipmentByWkeId(string id, Equipment eq)
        {
            if (id != null)
                Setup(o => o.GetEquipmentByWkeId(id)).Returns(Task.FromResult(eq));
            else Setup(o => o.GetEquipmentByWkeId(It.IsAny<string>())).Returns(Task.FromResult(eq));
            return this;
        }

        public MockEquipmentApiClientService ConfigureMockForGetEquipmentByWkeIdTracked(Equipment[] equipmentsToReturn)
        {
            _callTracker = 0;
            Setup(o => o.GetEquipmentByWkeId(It.IsAny<string>())).Callback(() =>
            {
                _callTracker++;
            }).Returns(() => Task.FromResult(equipmentsToReturn[_callTracker - 1]));
            return this;
        }
    }
}
