using System;
using Tlm.Sdk.Core.Models.Hypermedia;
using TLM.EHC.Common.Exceptions;

namespace TLM.EHC.API.ErrorExamples
{
    public static class EhcExampleErrors
    {
        public static Error NegativePageSize(string key, int intVal) => Error.InvalidParameter(key, "Invalid Page Size(1)", string.Format(EhcConstants.NegativePageSize, intVal));

        public static Error NegativePageNumber(string key, int intVal) => Error.InvalidParameter(key, "Invalid Page Number", string.Format(EhcConstants.NegativePageNumber, intVal));

        public static Error InvalidPageSize(string key, string stringVal) => Error.InvalidParameter(key, "Invalid Page Size(2)", string.Format(EhcConstants.InvalidPageSize,stringVal));

        public static Error PageSizeGreaterThanMax(string key, int requested, int max)
        {
            return Error.InvalidParameter(key, "Invalid Page Size(3) ", string.Format(EhcConstants.PageSizeGreaterThanMax, (object)requested, (object)max));
        }
        public static Error InvalidStartDate(string key, DateTime date) => Error.InvalidParameter(key, "Invalid Start Date", EhcConstants.InvalidStartDate);

        public static Error InvalidEndDate(string key, DateTime endDate) => Error.InvalidParameter(key, "Invalid End Date", EhcConstants.InvalidEndDate);

    }
}
