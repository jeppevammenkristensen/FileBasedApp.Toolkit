using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FileBasedApp.Toolkit;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(AbsoluteWebUriHttpExtensions))]
public class AbsoluteWebUriHttpExtensionsTest
{

    [Fact]
    public async Task GetAsync_SendsGetRequestToUri()
    {
        var handler = new TestHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://example.com/api/resource", request.RequestUri!.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("hello"),
            });
        });
        using var httpClient = new HttpClient(handler);
        var uri = AbsoluteWebUri.Create("https://example.com/api/resource");

        var response = await uri.GetAsync(httpClient);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("hello");
    }
    
    [Fact]
    public async Task GetAsync_SendsRequestWithOperator()
    {
        var handler = new TestHttpMessageHandler((request, _) =>
        {
            Assert.Equal(HttpMethod.Get, request.Method);
            Assert.Equal("https://example.com/api/resource?q=1", request.RequestUri!.ToString());
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("hello"),
            });
        });
        using var httpClient = new HttpClient(handler);
        var uri = AbsoluteWebUri.Create("https://example.com/api/resource") / UriQueryString.From("q=1");

        var response = await uri.GetAsync(httpClient);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).Should().BeEquivalentTo("hello");
    }

    private sealed class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _handler(request, cancellationToken);
        }
    }
}
