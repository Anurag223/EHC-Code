using System;
using System.Threading.Tasks;
using Moq;
using NSubstitute;
using TLM.EHC.API.Services;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace EHC.API.Tests.Mocks
{
    public class MockEquipmentModelProviderService : Mock<IEquipmentModelProvider>
    {
        private int _callTracker;

        public MockEquipmentModelProviderService MockEquipmentProviderServiceForGetEquipmentModelByCode(string id, EquipmentModel model)
        {
            if (String.IsNullOrEmpty(id))
                Setup(o => o.GetEquipmentModelByCode(It.IsAny<string>())).Returns(Task.FromResult(model));
            else Setup(o => o.GetEquipmentModelByCode(id)).Returns(Task.FromResult(model));
            return this;
        }

        public MockEquipmentModelProviderService MockEquipmentProviderServiceForGetEquipmentModelByCodeTracked(EquipmentModel[] modelsToReturn)
        {
            _callTracker = 0;
            Setup(o => o.GetEquipmentModelByCode(It.IsAny<string>())).Callback(() =>
            {
                _callTracker++;
            }).Returns(() => Task.FromResult(modelsToReturn[_callTracker - 1]));
            return this;
        }
    }
}
