using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using Tlm.Sdk.Api;
using Tlm.Sdk.Core.Models.Querying;

namespace EHC.API.Tests.ControllersTest
{
    public static class ApiBaseTest
    {
        public static void ValidateClassAttributes<TController>()
            where TController : CoreController
        {
            typeof(TController).Should().BeDecoratedWith<ApiVersionAttribute>
            (
                s => s.Versions.Count == 1 && s.Versions[0].ToString() == "2.0"
            );
            typeof(TController).Should().BeDecoratedWith<ApiExplorerSettingsAttribute>
            (
                s => s.GroupName == "v2"
            );
            typeof(TController).Should().BeDecoratedWith<RootPolicyAttribute>();
            typeof(TController).Should().BeDecoratedWith<RouteAttribute>();
        }

        public static void ValidateGetMethodsWithBadRequestAttributes<TController>()
            where TController : CoreController
        {
            typeof(TController).Methods()
                .ThatReturn<ActionResult>()
                .Should()
                .BeDecoratedWith<HttpGetAttribute>()
                .And.BeDecoratedWith<ProducesBadRequestResponseTypeAttribute>();
        }

        public static void ValidateGetMethodsWithNotAcceptableAttributes<TController>()
            where TController : CoreController
        {
            typeof(TController).Methods()
                .ThatReturn<ActionResult>()
                .Should()
                .BeDecoratedWith<HttpGetAttribute>()
                .And.BeDecoratedWith<ProducesNotAcceptableResponseTypeAttribute>();
        }

        public static void ValidateGetMethodsWithProduceOkAttributes<T, TController>()
            where T : class
            where TController : CoreController
        {
            typeof(TController).Methods()
                .ThatReturn<ActionResult>()
                .Should()
                .BeDecoratedWith<HttpGetAttribute>().And
                .BeDecoratedWith<ProducesOkResponseTypeAttribute>(s => s.Type == typeof(CollectionResult<T>));
        }
    }
}
