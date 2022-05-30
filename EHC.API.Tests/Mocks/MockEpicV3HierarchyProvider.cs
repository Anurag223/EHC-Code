using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using MoreLinq;
using NSubstitute;
using TLM.EHC.Common.Models;
using TLM.EHC.Common.Services;

namespace EHC.API.Tests.Mocks
{
    public class MockEpicV3HierarchyProvider : Mock<IEpicV3HierarchyProvider>
    {
        public MockEpicV3HierarchyProvider ConfigureMockForGetEpicHierarchyInfoFromCode(string id,
            EpicV3Hierarchy hierarchy)
        {
            if(String.IsNullOrEmpty(id))
                Setup(o => o.GetEpicHierarchyInfoFromCode(It.IsAny<string>()).Returns(Task.FromResult(hierarchy)));
            else Setup(o => o.GetEpicHierarchyInfoFromCode(id)).Returns(Task.FromResult(hierarchy));
            return this;
        }

        public MockEpicV3HierarchyProvider ConfigureMockForGetEpicHierarchyInfoFromCodeMultiParams(SortedList<string, EpicV3Hierarchy> paramList)
        {
            paramList.ForEach(delegate(KeyValuePair<string, EpicV3Hierarchy> pair)
            {
                Setup(o => o.GetEpicHierarchyInfoFromCode(pair.Key)).Returns(Task.FromResult(pair.Value));
            });
            return this;
        }
    }
}
