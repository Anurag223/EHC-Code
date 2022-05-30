using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.API.Common.ErrorExamples;
using TLM.EHC.API.ErrorExamples;

namespace EHC.API.Tests.ErrorExamplesTest
{
    [UnitTestCategory]
    [TestClass]
   public class EhcExampleErrorsTest
    {
        [TestMethod]
        public void Verify_NegativePageSize_Error()
        {
            var error = EhcExampleErrors.NegativePageSize("code", -1);
            Assert.AreEqual(error.Code, "INVALIDPAGESIZE1");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested page size is -1 which is less than one");

        }

        [TestMethod]
        public void Verify_NegativePageNumber_Error()
        {
            var error = EhcExampleErrors.NegativePageNumber("code", -1);
            Assert.AreEqual(error.Code, "INVALIDPAGENUMBER");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested page number is -1 which is less than one");

        }

        [TestMethod]
        public void Verify_InvalidPageSize_Error()
        {
            var error = EhcExampleErrors.InvalidPageSize("code", "-1");
            Assert.AreEqual(error.Code, "INVALIDPAGESIZE2");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested page size -1 is invalid");

        }

        [TestMethod]
        public void Verify_PageSizeGreaterThanMax_Error()
        {
            var error = EhcExampleErrors.PageSizeGreaterThanMax("code", 3,2);
            Assert.AreEqual(error.Code, "INVALIDPAGESIZE3");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested page size is 3 which is greater than the maximum size, 2");

        }

        [TestMethod]
        public void Verify_InvalidStartDate_Error()
        {
            var error = EhcExampleErrors.InvalidStartDate("code", new DateTime(2021,02,01));
            Assert.AreEqual(error.Code, "INVALIDSTARTDATE");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested start date is invalid");

        }

        [TestMethod]
        public void Verify_InvalidEndDate_Error()
        {
            var error = EhcExampleErrors.InvalidEndDate("code", new DateTime(2021, 02, 01));
            Assert.AreEqual(error.Code, "INVALIDENDDATE");
            Assert.AreEqual(error.Status, "400");
            Assert.AreEqual(error.StatusCode, 400);
            Assert.AreEqual(error.Detail, "The requested end date is invalid");

        }
    }
}
