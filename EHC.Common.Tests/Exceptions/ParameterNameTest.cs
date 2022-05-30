using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Testing.Unit;

namespace EHC.Common.Tests.Exceptions
{
    [UnitTestCategory]
    [TestClass()]
    public class ParameterNameTest
    {
        [TestMethod()]
        public void Test_GetParameterName_InvalidEndDate()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidEndDate);
            result.Should().Be("end");
        }

        [TestMethod()]
        public void Test_GetParameterName_InvalidStartDate()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidStartDate);
            result.Should().Be("start");
        }

        [TestMethod()]
        public void Test_GetParameterName_InvalidEquipmentWkeId()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidEquipmentWkeId);
            result.Should().Be("equipmentWkeId");
        }

        [TestMethod()]
        public void Test_GetParameterName_EquipmentNotFound()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.EquipmentNotFound);
            result.Should().Be("equipmentWkeId");
        }

        [TestMethod()]
        public void Test_GetParameterName_EpisodeNotFound()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.EpisodeNotFound);
            result.Should().Be("episode");
        }

        [TestMethod()]
        public void Test_GetParameterName_ChannelDefinitionNotFound()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.ChannelDefinitionNotFound);
            result.Should().Be("code");
        }

        [TestMethod()]
        public void Test_GetParameterName_ChannelCodeNotFound()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.ChannelCodeNotFound);
            result.Should().Be("code");
        }

        [TestMethod()]
        public void Test_GetParameterName_InvalidCode()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidCode);
            result.Should().Be("code");
        }

        [TestMethod()]
        public void Test_GetParameterName_EquipmentCodeNotFound()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.EquipmentCodeNotFound);
            result.Should().Be("equipmentcode");
        }

        [TestMethod()]
        public void Test_GetParameterName_InvalidFillValue()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidFillValue);
            result.Should().Be("fillValue");
        }

        [TestMethod()]
        public void Test_GetParameterName_InvalidThresholdTimestamp()
        {
            var result = ParameterName.GetParameterName(ErrorCodes.InvalidThresholdTimestamp);
            result.Should().Be("thresholdTimeStamp");
        }
        [TestMethod()]
        public void Test_GetParameterName_NoParameterDetails()
        {
            var result = ParameterName.GetParameterName(null);
            result.Should().Be("no parameter detail");
        }
    }
}
