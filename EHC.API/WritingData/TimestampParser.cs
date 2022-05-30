using System;
using TLM.EHC.API.Common;
using TLM.EHC.Common.Exceptions;
using Tlm.Sdk.Core;

namespace TLM.EHC.API.WritingData
{
    public interface ITimestampParser
    {
        DateTime Parse(object value);

        string ConvertToInfluxTimeStampInMilliseconds(string userProvidedDate);
    }


    public class TimestampParser : ITimestampParser
    {
        private readonly DateTime MinValue;
        private readonly DateTime MaxValue;
        private readonly long SecondsFrom1900to1970;

        public TimestampParser()
        {
            MinValue = new DateTime(2000, 01, 01);
            MaxValue = new DateTime(2035, 01, 01);

            var offset1900 = new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
            SecondsFrom1900to1970 = Math.Abs(offset1900.ToUnixTimeSeconds()); // 2208988800
        }


        public DateTime Parse(object value)
        {
            DateTime dateTime = ParseDateTime(value);

            if (!IsInValidRange(dateTime))
            {
                throw new BadRequestException("Timestamp is out of valid range: " + dateTime);
            }

            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }

            return dateTime;
        }


        private DateTime ParseDateTime(object value)
        {
            if (value == null)
            {
                throw new BadRequestException("Can't parse timestamp: null passed.");
            }

            if (value is DateTime dt)
            {
                return dt;
            }

            if (value is string str)
            {
                return ParseString(str);
            }

            if (value is double d)
            {
                return ParseDouble(d);
            }

            if (value is float)
            {
                // can't be float
            }

            if (value is long l)
            {
                return ParseLong(l);
            }

            if (value is int)
            {
                // can't be Int32
            }

            throw new BadRequestException($"Can't parse timestamp: unexpected data type '{ value.GetType() }'. Value is: { value }");

        }


        private DateTime ParseString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new BadRequestException("Can't parse timestamp: empty string.");
            }

            if (DateTime.TryParse(str, out DateTime dateTime))
            {
                return dateTime;
            }

            if (long.TryParse(str, out long l))
            {
                return ParseLong(l);
            }

            if (double.TryParse(str, out double d))
            {
                return ParseDouble(d);
            }

            throw new BadRequestException("Can't parse timestamp string: " + str);
        }


        private DateTime ParseDouble(double d)
        {
            try
            {
                return DateTime.FromOADate(d);
            }
            catch
            {
                throw new BadRequestException("Can't parse timestamp floating point number (DateTime.FromOADate): " + d);
            }
        }



        // may be also accept Unix epoch milliseconds?
        private DateTime ParseLong(long l)
        {
            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(l).UtcDateTime;

                if (IsInValidRange(dateTime))
                {
                    return dateTime;
                }

                long attempt2 = (l - SecondsFrom1900to1970);
                var dateTime2 = DateTimeOffset.FromUnixTimeSeconds(attempt2).UtcDateTime;

                if (IsInValidRange(dateTime2))
                {
                    return dateTime2;
                }

                throw new Exception();
            }
            catch
            {
                throw new BadRequestException("Can't parse timestamp integer number: " + l);
            }
        }




        private bool IsInValidRange(DateTime dateTime)
        {
            if (dateTime < MinValue)
            {
                return false;
            }

            if (dateTime > MaxValue)
            {
                return false;
            }

            return true;
        }

        public string ConvertToInfluxTimeStampInMilliseconds(string userProvidedDate)
        {
            userProvidedDate = string.IsNullOrEmpty(userProvidedDate)
                ? DateTimeOffset.Now.ToUnixTimeMilliseconds() + "000000"
                : DateTime.Parse(userProvidedDate).ToUnixTimeSeconds() + "000000000";
            return userProvidedDate;
        }

    }
}
