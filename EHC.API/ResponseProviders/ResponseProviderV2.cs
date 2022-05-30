using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using TLM.EHC.Common.Models;

namespace TLM.EHC.API.ResponseProviders
{
    public abstract class ResponseProviderV2 : ResponseProvider
    {
        protected TimePeriod GetResultPeriod(List<List<object>> rows)
        {
            DateTime begin = GetRowTimestamp(rows.First());
            DateTime end = GetRowTimestamp(rows.Last());
            return new TimePeriod(begin, end);
        }

        private DateTime GetRowTimestamp(List<object> rowData)
        {
            return (DateTime) rowData[0];
        }
    }
}
