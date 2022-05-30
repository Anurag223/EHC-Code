using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TLM.EHC.API;
using Tlm.Sdk.Api;
using TLM.EHC.API.Controllers;
using Tlm.Sdk.AspNetCore;
using Tlm.Sdk.Data.Mongo;
using Tlm.Sdk.Testing.Unit;

namespace EHC.API.Tests
{
    [UnitTestCategory]
    [TestClass]
    public class HREFTests
    {
        public static HostingConfig TestHostingConfig => HostingConfig.Instance;
        [TestMethod]
        public void MakeTemporaryUri_GivenPath_ReturnsTemporaryUri()
        {
            // arrange
            var (_, builder, accessor) = CreateUriBuilder("http", "localhost", TestHostingConfig.BaseUrl, "http", null);
            accessor.HttpContext.Request.Path = "/localhost/v2/channel-definitions";

            // act
            var uri = builder.MakeTemporaryUri<ChannelDefinitionsController>();

            // assert
            uri.ToString().Should().MatchRegex("http://localhost/v2/channel-definitions/requests/[0-9a-f]{24}");
        }
        [TestMethod]
        public void MakeUriFromPath_RelatedResourceNonProdEnvironment_UriConstructedCorrectly()
        {
            var (_, builder, accessor) = CreateUriBuilder(null, null, "http://xyz.com", "http", "/channeldefinitions/abc");
            accessor.HttpContext.Request.Headers.Add("mateo-origin-host", "evd.apigateway.slb.com");
            accessor.HttpContext.Request.Headers.Add("mateo-origin-proto", "https");
            accessor.HttpContext.Request.Headers.Add("mateo-origin-resource", "channel-definitions");
            accessor.HttpContext.Request.Headers.Add("mateo-origin-basepath", "a2r-ehc-historian/v2/");
            
            var uri = builder.MakeUriFromPath<ChannelDefinitionsController>("channel-definitions", UriPathRoot.FromRelatedApiPath);

            uri.ToString().Should().Be("https://evd.apigateway.slb.com/a2r-ehc-historian/v2/channel-definitions");
        }


        protected (HostingConfig Config, Tlm.Sdk.AspNetCore.UriBuilder Builder, MockContextAccessor Accessor) CreateUriBuilder(
            string configScheme,
            string configHost,
            string configBaseUrl,
            string requestScheme,
            string requestPath,
            string requestQueryString = null)
        {
            var config = new HostingConfig { Scheme = configScheme, HostName = configHost, BaseUrl = configBaseUrl };
            config.Providers = new[] { UriPartProviderId.ForwardedHeaders, UriPartProviderId.MateoOriginsHeaders,
                UriPartProviderId.Config, UriPartProviderId.XForwardedHeaders, UriPartProviderId.XOriginalHeaders, UriPartProviderId.HttpRequest };

            var accessor = new MockContextAccessor();
            accessor.HttpContext.Request.QueryString = requestQueryString != null ? new QueryString(requestQueryString) : QueryString.Empty;
            accessor.HttpContext.Request.Path = requestPath;
            accessor.HttpContext.Request.Scheme = requestScheme ?? configScheme;

            var b = new Tlm.Sdk.AspNetCore.UriBuilder(
                config,
                accessor,
                new KeyGenerator<Tlm.Sdk.Core.Models.Infrastructure.Request>(),
                new IUriPartProvider[]
                {
                    new EHCConfigUriPartProvider(accessor,config),
                    new ForwardedHeadersUriPartProvider(accessor, config),
                    new MateoOriginsHeadersUriPartProvider(accessor, config),
                    new XForwardedHeadersUriPartProvider(accessor, config),
                    new XOriginalHeadersUriPartProvider(accessor, config),
                    new HttpRequestUriPartProvider(accessor, config),
                });

            return (config, b, accessor);
        }



    }
}
