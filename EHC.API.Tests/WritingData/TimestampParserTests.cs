using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Common;
using TLM.EHC.API.WritingData;
using TLM.EHC.Common.Exceptions;

namespace EHC.API.Tests.WritingData
{
    [UnitTestCategory]
    [TestClass]
    public class TimestampParserTests
    {
        private static TimestampParser _parser;
        private static DateTime _expectedValueMilliseconds;
        private static DateTime _expectedValueSeconds;


        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _parser = new TimestampParser();

            // let's expect 2019-07-29 23:39:12.738 UTC if precision is milliseconds
            _expectedValueMilliseconds = new DateTime(2019, 07, 29, 23, 39, 12, 738, DateTimeKind.Utc);

            // let's expect 2019-07-29 23:39:12.000 UTC if precision is seconds
            _expectedValueSeconds = new DateTime(2019, 07, 29, 23, 39, 12, 000, DateTimeKind.Utc); 
        }


        [ClassCleanup]
        public static void ClassCleanup()
        {
            _parser = null;
        }

        [TestMethod]
        public void Unix_Long()
        {
            DateTime dateTime = _parser.Parse(1564443552L);
            dateTime.Should().Be(_expectedValueSeconds);
        }

        [TestMethod]
        public void Unix_Long_as_String()
        {
            DateTime dateTime = _parser.Parse("1564443552");
            dateTime.Should().Be(_expectedValueSeconds);
        }

        [TestMethod]
        public void Since1900_Long()
        {
            DateTime dateTime = _parser.Parse(3773432352L);
            dateTime.Should().Be(_expectedValueSeconds);
        }

        [TestMethod]
        public void OLE_Automation_Double()
        {
            DateTime dateTime = _parser.Parse(43675.985564097224D);
            dateTime.Should().Be(_expectedValueMilliseconds);
        }

        [TestMethod]
        public void ISO_8601_String()
        {
            DateTime dateTime = _parser.Parse("2019-07-29T23:39:12.738Z");
            dateTime.Should().Be(_expectedValueMilliseconds);
        }


        [TestMethod]
        public void AmericanFormat_String()
        {
            DateTime dateTime = _parser.Parse("7/29/2019 11:39:12.738 PM");
            dateTime.Should().Be(_expectedValueMilliseconds);
        }


        [TestMethod]
        public void OutOfRange_Min_Exception()
        {
            _parser.Invoking(p => p.Parse("1999-07-27T23:39:12.738Z"))
                .Should().Throw<BadRequestException>();
        }


        [TestMethod]
        public void OutOfRange_Max_Exception()
        {
            _parser.Invoking(p => p.Parse("2040-08-03T23:39:12.738Z"))
                .Should().Throw<BadRequestException>();
        }


        [TestMethod]
        public void InvalidDataType_int_Exception()
        {
            _parser.Invoking(p => p.Parse(123))
                .Should().Throw<BadRequestException>();
        }


        [TestMethod]
        public void InvalidValue_long_Exception()
        {
            _parser.Invoking(p => p.Parse(100_000_000L))
                .Should().Throw<BadRequestException>();
        }
    }
}
