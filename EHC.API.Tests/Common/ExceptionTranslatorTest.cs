using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Common;
using TLM.EHC.Common.Exceptions;

namespace EHC.API.Tests.Common
{
    [UnitTestCategory]
    [TestClass]
   public class ExceptionTranslatorTest
    {
        [TestMethod]
        public void TestForBadRequestException()
        {
            var exception=ExceptionTranslator.GetExceptionType(EhcConstants.PartialWriteError);
            exception.Should().BeOfType<BadRequestException>();
            exception.Message.Should().Be($"{ EhcConstants.WriteSuffix}{EhcConstants.PartialWriteError}");
        }
        [TestMethod]
        public void TestForServerErrorException()
        {
            var exception = ExceptionTranslator.GetExceptionType("Any exception message");
            exception.Should().BeOfType<ServerErrorException>();
            exception.Message.Should().Be($"{ EhcConstants.WriteSuffix}Any exception message");

        }
    }
}
