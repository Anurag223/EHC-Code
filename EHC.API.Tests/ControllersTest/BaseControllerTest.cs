using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using TLM.EHC.API.WritingData;
using TLM.EHC.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.Common.Exceptions;
using TLM.EHC.Common.Models;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests.ControllersTest
{
    [UnitTestCategory]
    [TestClass]
    public class BaseControllerTest 
    {
        private MockRepository _mockProvider;
        private Mock<ITimestampParser> _mockTimeStampParser;
        private MyUnitTestController _controller;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockProvider = new MockRepository(MockBehavior.Loose);
            _mockTimeStampParser = _mockProvider.Create<ITimestampParser>();
            _controller = CreateControllerObject();
        }

        [TestCleanup]
        public void SetUpTestMethod()
        {
            _controller = null;
        }

        [TestMethod]
        public void Verify_ActionResult_CreateHttpStatus_ForNotFoundException_WithErrorCode()
        {
            var _exception = new NotFoundException("Error Message " + "test") { ErrorCode = ErrorCodes.ChannelDefinitionNotFound };
            var result = _controller.ExposeCreateHttpStatus(_exception);
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();
            (((ObjectResult)result).StatusCode).Should().Be(404);
            ((Error)(((ObjectResult)result).Value)).Code.Should().Be("CHANNELDEFINITIONNOTFOUND");
            ((Error)(((ObjectResult)result).Value)).Title.Should().Be("ChannelDefinitionNotFound");
            ((Error)(((ObjectResult)result).Value)).Detail.Should().Be("Error Message test");
        }

        [TestMethod]
        public void Verify_ActionResult_CreateHttpStatus_ForNotFoundException_WithOutErrorCode()
        {
            var _exception = new NotFoundException("Exception message");
            var result = _controller.ExposeCreateHttpStatus(_exception);
            result.Should().NotBeNull();
            result.Should().BeOfType<NotFoundObjectResult>();
            (((ObjectResult)result).StatusCode).Should().Be(404);
            ((Error) (((ObjectResult) result).Value)).Code.Should().Be("NOTFOUND");
            ((Error)(((ObjectResult)result).Value)).Title.Should().Be("NotFound");
            ((Error)(((ObjectResult)result).Value)).Detail.Should().Be("Exception message");
        }

        [TestMethod]
        public void Verify_ActionResult_CreateHttpStatus_ForBadRequestException_WithErrorCode()
        {
            var _exception = new BadRequestException("Error Message " + "test") { ErrorCode = ErrorCodes.ChannelDefinitionNotFound };
            var result = _controller.ExposeCreateHttpStatus(_exception);
            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
            (((ObjectResult)result).StatusCode).Should().Be(400);
            ((Error)(((ObjectResult)result).Value)).Code.Should().Be("CHANNELDEFINITIONNOTFOUND");
            ((Error)(((ObjectResult)result).Value)).Title.Should().Be("ChannelDefinitionNotFound");
            ((Error)(((ObjectResult)result).Value)).Detail.Should().Be("Error Message test");
        }

        [TestMethod]
        public void Verify_ActionResult_CreateHttpStatus_ForBadRequestException_WithOutErrorCode()
        {
            var _exception = new BadRequestException("Exception message");
            var result = _controller.ExposeCreateHttpStatus(_exception);
            result.Should().NotBeNull();
            result.Should().BeOfType<BadRequestObjectResult>();
            (((ObjectResult)result).StatusCode).Should().Be(400);
            ((Error)(((ObjectResult)result).Value)).Code.Should().Be("BADREQUEST");
            ((Error)(((ObjectResult)result).Value)).Title.Should().Be("BadRequest");
            ((Error)(((ObjectResult)result).Value)).Detail.Should().Be("Exception message");
        }

        [TestMethod]
        public void Verify_ActionResult_CreateHttpStatus_ForServerErrorException()
        {
            var _exception = new ServerErrorException("Exception message");
            var result = _controller.ExposeCreateHttpStatus(_exception);
            result.Should().NotBeNull();
            (((ObjectResult)result).StatusCode).Should().Be(500);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Time period Start/End should be both specified or both omitted.")]
        public void VerifyException_TimePeriod_WhenStartIsNull()
        {
            _controller.ExposeParseTimePeriod(null, new DateTime(2021, 1, 1).ToString("d"));
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Default value of DateTime passed.")]
        public void VerifyException_TimePeriod_WhenStartIsGreaterThanEnd()
        {
           var result= _controller.ExposeParseTimePeriod(new DateTime(2021, 2, 2).ToString("d"), new DateTime(2021, 1, 1).ToString("d"));
        }

        [TestMethod]
        public void Verify_TimePeriod_WhenSuccessful()
        {
            var result = _controller.ExposeParseTimePeriod("2019-04-01", "2019-05-06");
            result.Should().NotBeNull();
            result.Should().BeOfType<TimePeriod>();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Verify_ConvertToActionResult_WhenException()
        {
            _controller.ExposeConvertToActionResult(null);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Can't parse timestamp string: 2021-02-0")]
        public void Verify_ConvertToActionResult_WhenInvalidEndDate()
        {
            _controller.ExposeParseTimePeriod("2021-02-01", "2021-02-0");
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException), "Can't parse timestamp string: 2021-02-0")]
        public void Verify_ConvertToActionResult_WhenInvalidStartDate()
        {
            _controller.ExposeParseTimePeriod("2021-02-0", "2021-02-01");
        }

        private MyUnitTestController CreateControllerObject()
        {
            return new MyUnitTestController(new TimestampParser());
        }
    }

    public class MyUnitTestController : BaseController
    {
        public MyUnitTestController(ITimestampParser timestampParser) : base(timestampParser)
        {
        }

        public IActionResult ExposeCreateHttpStatus(HttpStatusException ex)
        {
           return base.CreateHttpStatus(ex);
        }

        public TimePeriod ExposeParseTimePeriod(string start, string end)
        {
            return base.ParseTimePeriod(start,end);
        }

        public ActionResult ExposeConvertToActionResult(ApiResponse apiResponse)
        {
            return base.ConvertToActionResult(apiResponse);
        }
    }
}
