using System;

namespace TLM.EHC.Common.Exceptions
{
    public class EhcApiException : Exception
    {
        public ErrorCodes? ErrorCode { get; set; }

        public EhcApiException(string message) : base(message)
        {
        }


        public EhcApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
