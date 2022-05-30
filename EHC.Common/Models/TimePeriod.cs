using System;

namespace TLM.EHC.Common.Models
{
    /// <summary>
    /// Timeperiod with start and end time.
    /// </summary>
    public class TimePeriod
    {
        /// <summary>
        /// Start time.
        /// </summary>
        public DateTime Start { get; }
        /// <summary>
        /// End time.
        /// </summary>
        public DateTime End { get; }


        public TimePeriod(DateTime start, DateTime end)
        {
            if (start == default(DateTime) || end == default(DateTime))
            {
                throw new ArgumentException("Default value of DateTime passed.");
            }

            if (start > end)
            {
                throw new ArgumentException($"Time period Start { start } can't be later than End { end }.");
            }

            if (start.Kind == DateTimeKind.Unspecified)
            {
                start = DateTime.SpecifyKind(start, DateTimeKind.Utc);
            }

            if (end.Kind == DateTimeKind.Unspecified)
            {
                end = DateTime.SpecifyKind(end, DateTimeKind.Utc);
            }

            this.Start = start.ToUniversalTime();
            this.End = end.ToUniversalTime();
        }


        public TimePeriod GetPrevious24h()
        {
            return new TimePeriod(Start.AddDays(-1), Start);
        }
    }
}
