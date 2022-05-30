using System.Threading.Tasks;
using Moq;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace EHC.API.Tests.Mocks
{
    public class MockEquipmentProviderService : Mock<IEquipmentProvider>
    {
        private int _callTracker;

        public MockEquipmentProviderService ConfigureMockForGetEquipmentByWkeId(WellKnownEntityId id, Equipment eq)
        {
            if(id != null)
                Setup(o => o.GetEquipmentByWkeid(id)).Returns(Task.FromResult(eq));
            else Setup(o => o.GetEquipmentByWkeid(It.IsAny<WellKnownEntityId>())).Returns(Task.FromResult(eq));
            return this;
        }

        public MockEquipmentProviderService ConfigureMockForGetEquipmentByWkeIdTracked(Equipment[] equipmentsToReturn)
        {
            _callTracker = 0;
            Setup(o => o.GetEquipmentByWkeid(It.IsAny<WellKnownEntityId>())).Callback(() =>
            {
                _callTracker++;
            }).Returns(() => Task.FromResult(equipmentsToReturn[_callTracker - 1]));
            return this;
        }
    }
}
