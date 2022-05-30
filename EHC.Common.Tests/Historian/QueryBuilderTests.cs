using System;
using System.Globalization;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tlm.Sdk.Testing.Unit;
using TLM.EHC.Common.Historian;
using TLM.EHC.Common.Models;

namespace EHC.Common.Tests.Historian
{
    [UnitTestCategory]
    [TestClass()]
    public class QueryBuilderTests
    {
        [TestMethod()]
        public void Test_SelectAllFields()
        {
            var query = new QueryBuilder()
                .UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel)
                .SelectAllFields()
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();

            query.Database.Should().Be("DXJ_WPS_BLENDING_EQUIPMENT");
            query.SelectText.Should().Be("SELECT * FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneField()
        {
            var query = new QueryBuilder()
                .UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel)
                .SelectOneField("AirPressure")
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();

            query.Database.Should().Be("DXJ_WPS_BLENDING_EQUIPMENT");
            query.SelectText.Should().Be("SELECT \"AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMeanAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure",AggregationFunctions.Mean)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT mean(\"AirPressure\") AS \"mean_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMeanAggregateFunction_WithGroupByAndNoneAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Mean)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT mean(\"AirPressure\") AS \"mean_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithFirstAggregateFunction_WithGroupByAndPreviousAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.First)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("previous")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT first(\"AirPressure\") AS \"first_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(previous)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithLastAggregateFunction_WithGroupByAndPreviousAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Last)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("previous")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT last(\"AirPressure\") AS \"last_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(previous)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMeanAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Mean)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT mean(\"AirPressure\") AS \"mean_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMinAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Min)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT min(\"AirPressure\") AS \"min_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMinAggregateFunction_WithGroupByAndLinearAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Min)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("linear")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT min(\"AirPressure\") AS \"min_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(linear)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMinAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Min)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT min(\"AirPressure\") AS \"min_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMaxAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Max)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT max(\"AirPressure\") AS \"max_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMaxAggregateFunction_WithGroupByAndPreviousAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Max)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("previous")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT max(\"AirPressure\") AS \"max_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(previous)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMaxAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Max)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT max(\"AirPressure\") AS \"max_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithCountAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Count)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT count(\"AirPressure\") AS \"count_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithCountAggregateFunction_WithGroupByAndNumberAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Count)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT count(\"AirPressure\") AS \"count_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(1)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithCountAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Count)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT count(\"AirPressure\") AS \"count_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMedianAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Median)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT median(\"AirPressure\") AS \"median_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMedianAggregateFunction_WithGroupByAndNumberAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Median)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT median(\"AirPressure\") AS \"median_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(1)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithMedianAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Median)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT median(\"AirPressure\") AS \"median_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSumAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Sum)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT sum(\"AirPressure\") AS \"sum_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSumAggregateFunction_WithGroupByAndNumberAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Sum)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT sum(\"AirPressure\") AS \"sum_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(1)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSumAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Sum)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT sum(\"AirPressure\") AS \"sum_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSpreadAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Spread)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT spread(\"AirPressure\") AS \"spread_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSpreadAggregateFunction_WithGroupByAndNumberAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Spread)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT spread(\"AirPressure\") AS \"spread_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(1)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithSpreadAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Spread)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT spread(\"AirPressure\") AS \"spread_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithStddevAggregateFunction_WithGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Stddev)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10h")
                .UseFill()
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT stddev(\"AirPressure\") AS \"stddev_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10h) FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectOneFieldWithStddevAggregateFunction_WithGroupByAndNumberAsFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Stddev)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10d")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT stddev(\"AirPressure\") AS \"stddev_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10d) FILL(1)");
        }

        [TestMethod()]
        public void Test_SelectOneFieldWithStddevAggregateFunction_WithoutGroupByAndNoFillValue()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectOneFieldWithAggregateFunction("AirPressure", AggregationFunctions.Stddev)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT stddev(\"AirPressure\") AS \"stddev_AirPressure\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000");
        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithMeanAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Mean)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT mean(\"AssetNumber\") AS \"mean_AssetNumber\",mean(\"AirCompressorRunStatus\") AS \"mean_AirCompressorRunStatus\",mean(\"ActiveVoltage_PTPOD.Volt\") AS \"mean_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithMinAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Min)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT min(\"AssetNumber\") AS \"min_AssetNumber\",min(\"AirCompressorRunStatus\") AS \"min_AirCompressorRunStatus\",min(\"ActiveVoltage_PTPOD.Volt\") AS \"min_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithMaxAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Max)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT max(\"AssetNumber\") AS \"max_AssetNumber\",max(\"AirCompressorRunStatus\") AS \"max_AirCompressorRunStatus\",max(\"ActiveVoltage_PTPOD.Volt\") AS \"max_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithCountAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Count)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT count(\"AssetNumber\") AS \"count_AssetNumber\",count(\"AirCompressorRunStatus\") AS \"count_AirCompressorRunStatus\",count(\"ActiveVoltage_PTPOD.Volt\") AS \"count_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithSumAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Sum)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT sum(\"AssetNumber\") AS \"sum_AssetNumber\",sum(\"AirCompressorRunStatus\") AS \"sum_AirCompressorRunStatus\",sum(\"ActiveVoltage_PTPOD.Volt\") AS \"sum_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithMedianAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Median)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT median(\"AssetNumber\") AS \"median_AssetNumber\",median(\"AirCompressorRunStatus\") AS \"median_AirCompressorRunStatus\",median(\"ActiveVoltage_PTPOD.Volt\") AS \"median_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithStddevAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Stddev)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT stddev(\"AssetNumber\") AS \"stddev_AssetNumber\",stddev(\"AirCompressorRunStatus\") AS \"stddev_AirCompressorRunStatus\",stddev(\"ActiveVoltage_PTPOD.Volt\") AS \"stddev_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectMultipleFieldsWithSpreadAggregateFunction()
        {
            string[] codes = new[] { "AssetNumber", "AirCompressorRunStatus", "ActiveVoltage_PTPOD.Volt" };
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithAggregationFunction(codes, AggregationFunctions.Spread)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("none")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT spread(\"AssetNumber\") AS \"spread_AssetNumber\",spread(\"AirCompressorRunStatus\") AS \"spread_AirCompressorRunStatus\",spread(\"ActiveVoltage_PTPOD.Volt\") AS \"spread_ActiveVoltage_PTPOD.Volt\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithMeanAggregateFunction()
        {
           var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Mean)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT mean(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithMaxAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Max)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT max(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithMinAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Min)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT min(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithCountAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Count)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT count(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithMedianAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Median)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT median(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithSumAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Sum)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT sum(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithSpreadAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Spread)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT spread(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod()]
        public void Test_SelectAllFieldsWithStddevAggregateFunction()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectAllFieldsWithAggregationFunction(AggregationFunctions.Stddev)
                .FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14)))
                .UseGroupBy("10s")
                .UseFill("1")
                .GetQuery();
            query.SelectText.Should()
                .Be(
                    "SELECT stddev(*) FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 GROUP BY time(10s) FILL(1)");

        }

        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenNanosecondsAreNotPresent()
        {
            const string dummyDateTime = "2021-12-09T17:06:21.810Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result= QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
           result.Should().Be("1639069581810000000");
        }

        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenMilliAndNanosecondsAreNotPresent()
        {
            const string dummyDateTime = "2021-12-09T17:06:21Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result = QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
            result.Should().Be("1639069581000000000");
        }

        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenNanosecondsArePresent()
        {
            const string dummyDateTime = "2021-12-09T17:06:21.810145Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result = QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
            result.Should().Be("1639069581810145000");

        }

        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenNanosecondsArePresentWithValueUptoFourPrecision()
        {
            const string dummyDateTime = "2021-12-09T17:06:21.8101456Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result = QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
            result.Should().Be("1639069581810145600");

        }

        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenNanosecondsArePresentWithValueUptoSixPrecision()
        {
            const string dummyDateTime = "2021-12-09T17:06:21.810145678Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result = QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
            result.Should().Be("1639069581810145700");

        }
        [TestMethod]
        public void Test_ConvertToUnixNanoseconds_WhenNanosecondsArePresentWithValueUptoEightPrecision()
        {
            const string dummyDateTime = "2021-12-09T17:06:21.81014567820Z";
            var parsedDate = DateTime.Parse(dummyDateTime,
                CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
            var result = QueryBuilder.ConvertToUnixNanoseconds(parsedDate);
            result.Should().Be("1639069581810145700");

        }

        [TestMethod()]
        public void Test_SelectFieldsWithMathFunctionWithAddOperation_Successful()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).               
                SelectFieldsWithMathFunction("ThrottleAct.RPM", "ThrottleSp.RPM",MathFunctions.Add,"ThrottleAct","ThrottleSp").FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14))).UseFill()
                .GetQuery();

            query.SelectText.Should().Be(
                   "SELECT \"ThrottleAct.RPM\" + \"ThrottleSp.RPM\" AS \"ThrottleAct_Add_ThrottleSp\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 FILL(null)");

        }

        [TestMethod()]
        public void Test_SelectFieldsWithMathFunctionWithSubtractOperation_Successful()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithMathFunction("ThrottleAct.RPM", "ThrottleSp.RPM", MathFunctions.Subtract, "ThrottleAct", "ThrottleSp").FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14))).UseFill("none")
                .GetQuery();

            query.SelectText.Should().Be(
                "SELECT \"ThrottleAct.RPM\" - \"ThrottleSp.RPM\" AS \"ThrottleAct_Subtract_ThrottleSp\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 FILL(none)");

        }

        [TestMethod()]
        public void Test_SelectFieldsWithMathFunctionWithMultiplyOperation_Successful()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithMathFunction("ThrottleAct.RPM", "ThrottleSp.RPM", MathFunctions.Multiply, "ThrottleAct", "ThrottleSp").FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14))).UseFill("previous")
                .GetQuery();

            query.SelectText.Should().Be(
                "SELECT \"ThrottleAct.RPM\" * \"ThrottleSp.RPM\" AS \"ThrottleAct_Multiply_ThrottleSp\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 FILL(previous)");

        }

        [TestMethod()]
        public void Test_SelectFieldsWithMathFunctionWithDivideOperation_Successful()
        {
            var query = new QueryBuilder().UseTechnology("DXJ_WPS_BLENDING_EQUIPMENT")
                .UseBrand("WS-63_STIMULATION_BLENDER_PROP")
                .UseDataType(DataType.Channel).
                SelectFieldsWithMathFunction("ThrottleAct.RPM", "ThrottleSp.RPM", MathFunctions.Divide, "ThrottleAct", "ThrottleSp").FilterByWkeId("100196736:SBF62412A0281")
                .FilterByTimePeriod(new TimePeriod(new DateTime(2019, 08, 12), new DateTime(2019, 08, 14))).UseFill("linear")
                .GetQuery();

            query.SelectText.Should().Be(
                "SELECT \"ThrottleAct.RPM\" / \"ThrottleSp.RPM\" AS \"ThrottleAct_Divide_ThrottleSp\" FROM \"WS-63_STIMULATION_BLENDER_PROP\" WHERE EquipmentInstance='100196736:SBF62412A0281' AND time >= 1565568000000000000 AND time <= 1565740800000000000 FILL(linear)");

        }

    }
}