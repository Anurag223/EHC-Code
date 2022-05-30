using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Castle.Core.Internal;
using TLM.EHC.Common.Models;

// ReSharper disable once CheckNamespace
namespace TLM.EHC.Common.Historian
{
    [Serializable]
    public class QueryBuilder
    {
        private string _technology; // database
        private string _brand;      // table (measurement)
        private string _dataTypeSuffix;

        private readonly List<string> _fields = new List<string>();
        private readonly List<string> _filters = new List<string>();
        private readonly StringBuilder _groupByValue=new StringBuilder();
        private readonly StringBuilder _fillValue=new StringBuilder();
        private const decimal TicksPerNanoseconds = TimeSpan.TicksPerMillisecond / 1000000m;


        public QueryBuilder UseTechnology(string technology) // database
        {
            if (string.IsNullOrWhiteSpace(technology))
            {
                throw new ArgumentException("Empty technology value.");
            }

            _technology = technology;
            return this;
        }

        public QueryBuilder UseBrand(string brand) // table (measurement)
        {
            if (string.IsNullOrWhiteSpace(brand))
            {
                throw new ArgumentException("Empty brand value.");
            }

            _brand = brand;
            return this;
        }


        public QueryBuilder UseDataType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Channel:
                    _dataTypeSuffix = "";
                    break;
                case DataType.Reading:
                    _dataTypeSuffix = ".Reading";
                    break;
                case DataType.Episodic:
                    _dataTypeSuffix = ".Episodic";
                    break;
                    default:
                        throw new ArgumentException("Unexpected DataType: " + dataType);
            }

            return this;
        }


        public QueryBuilder SelectAllFields()
        {
            _fields.Add("*");
            return this;
        }

        public QueryBuilder SelectAllFieldsWithAggregationFunction(AggregationFunctions? funcName)
        {
            _fields.Add(funcName?.ToString().ToLower()+ "(" + "*" + ")");
            return this;
        }

        public QueryBuilder SelectOneField(string field)
        {
            _fields.Add(Quote(field));
            return this;
        }

        public QueryBuilder SelectFields(string[] fields)
        {
            _fields.AddRange(fields.Select(Quote));
            return this;
        }

        public QueryBuilder SelectOneFieldWithAggregateFunction(string field, AggregationFunctions? funcName)
        {
            _fields.Add(Aggregate(field,funcName));
            return this;
          
        }
        public QueryBuilder SelectFieldsWithAggregationFunction(string[] fields, AggregationFunctions? funcName)
        {
            _fields.AddRange(fields.Select(x=>Aggregate(x,funcName)));
            return this;
        }

        /// <summary>
        /// This function creates select clause with Math functions.
        /// </summary>
        /// <param name="firstCode">First operand with uom.</param>
        /// <param name="secondCode">Second operand with uom.</param>
        /// <param name="funcName">Operation name as enum values.</param>
        /// <param name="firstCodename">First operand without uom.</param>
        /// <param name="secondCodeName">Second operand without uom.</param>
        /// <returns>A QueryBuilder object. </returns>
        public QueryBuilder SelectFieldsWithMathFunction(string firstCode,string secondCode, MathFunctions funcName,string firstCodename,string secondCodeName)
        {
            var res= PrepareSelectQueryBasedOnMathFunction(firstCode, secondCode, funcName,firstCodename,secondCodeName);
            _fields.Add(res);
            return this;
        }
        private string PrepareSelectQueryBasedOnMathFunction(string firstCode, string secondCode, MathFunctions funcName,string firstCodename, string secondCodeName )
        {
            var op = funcName switch
            {
                MathFunctions.Add => "+",
                MathFunctions.Subtract => "-",
                MathFunctions.Divide => "/",
                MathFunctions.Multiply => "*",
                _ => null
            };
            var resultName = $"{firstCodename}_{funcName}_{secondCodeName}";
            var value = $"{Quote(firstCode)} {op} {Quote(secondCode)} AS {Quote(resultName)}";
            return value;
        }

        public QueryBuilder ForceEpisodeField()
        {
            _fields.Insert(0, "Episode");
            return this;
        }

        public QueryBuilder FilterByWkeId(string wkeid)
        {
            _filters.Add($"EquipmentInstance={ QuoteSingle(wkeid) }");
            return this;
        }

        public QueryBuilder FilterByTimePeriod(TimePeriod timePeriod)
        {
            _filters.Add("time >= " + ConvertToUnixNanoseconds(timePeriod.Start));
            _filters.Add("time <= " + ConvertToUnixNanoseconds(timePeriod.End));
            return this;
        }

        public QueryBuilder UseGroupBy(string value)
        {
            _groupByValue.Append(" GROUP BY time(" + value + ")");
            return this;
        }

        public QueryBuilder UseFill(string value=null)
        {
            if(value.IsNullOrEmpty())
            {
                _fillValue.Append(" FILL(" + "null" + ")");
            }
            else
            {
                _fillValue.Append(" FILL(" + value?.ToLower() + ")");
            }
           
            return this;
        }

        public QueryBuilder FilterByEpisodeId(string episodeId)
        {
            _filters.Add($"Episode={ QuoteSingle(episodeId) }");
            return this;
        }

        private string Quote(string value)
        {
            return '\"' + value + '\"';
        }

        private string Aggregate(string value, AggregationFunctions? funcName)
        {          
            return funcName.ToString()?.ToLower() + '(' + Quote(value) + ')'+ " AS " + Quote(funcName.ToString()?.ToLower()+'_'+value);
        }

        private string QuoteSingle(string value)
        {
            return '\'' + value + '\'';
        }


        public Query GetQuery()
        {
            var sb = new StringBuilder();

            sb.Append("SELECT ");
            sb.Append(string.Join(',', _fields));

            if (_dataTypeSuffix == null)
            {
                throw new InvalidOperationException("No data type chosen.");
            }

            sb.Append(" FROM ");
            sb.Append(Quote(_brand + _dataTypeSuffix));


            if (_filters.Count == 0)
            {
                throw new InvalidOperationException("No filters.");
            }

            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", _filters));
            sb.Append(_groupByValue);
            sb.Append(_fillValue);

            return new Query(_technology, sb.ToString());
        }


        public Query GetQueryForLatestTimestamp()
        {
            var query = GetQuery();
            var selectText = query.SelectText + " ORDER BY DESC LIMIT 1";

            return new Query(query.Database, selectText);
        }

        public Query GetQueryShowAllFields()
        {
            if (_dataTypeSuffix == null)
            {
                throw new InvalidOperationException("No data type chosen.");
            }

            string selectText = "SHOW FIELD KEYS FROM " + Quote(_brand + _dataTypeSuffix);
            return new Query(_technology, selectText);
        }


        
        // influx multi query
        // select first found value for every field
        // so if null - that field has no any data for given wkeid
        public Query GetMultiQueryAllFieldsValues()
        {
            if (_fields.Count == 0)
            {
                throw new ArgumentException("Empty allFields array.");
            }

            var sb = new StringBuilder();

            foreach (var field in _fields)
            {
                sb.Append("SELECT ");
                sb.Append(field);

                if (_dataTypeSuffix == null)
                {
                    throw new InvalidOperationException("No data type chosen.");
                }

                sb.Append(" FROM ");
                sb.Append(Quote(_brand + _dataTypeSuffix));

                if (_filters.Count == 0)
                {
                    throw new InvalidOperationException("No filters.");
                }

                sb.Append(" WHERE ");
                sb.Append(string.Join(" AND ", _filters));

                sb.Append(" LIMIT 1;");
            }

            return new Query(_technology, sb.ToString());
        }


        /// <summary>
        /// influx queries take nanoseconds since unix epoch
        /// </summary>
        public static string ConvertToUnixNanoseconds(DateTime dateTime)
        {
            var utc = dateTime.ToUniversalTime();
            var nanoseconds = GetNanoseconds(utc.TimeOfDay).ToString(CultureInfo.InvariantCulture);
            if (nanoseconds is "0")
            {
                nanoseconds = "000000";
            }
            var offset = new DateTimeOffset(utc);
            return offset.ToUnixTimeMilliseconds() + nanoseconds;
        }

        private static decimal GetNanoseconds(TimeSpan ts) => ts.Ticks % TimeSpan.TicksPerMillisecond / TicksPerNanoseconds;

      
    }
}
