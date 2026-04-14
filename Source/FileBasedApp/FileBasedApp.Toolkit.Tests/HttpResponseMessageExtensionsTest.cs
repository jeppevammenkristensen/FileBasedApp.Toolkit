using System;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(HttpResponseMessageExtensions))]
public partial class HttpResponseMessageExtensionsTest
{

    [Fact]
    public async Task ToJson_DeserializesResponseContent()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent("""{"name":"widget","count":3}""");

        var result = await response.ToJson(TestJsonContext.Default.SamplePayload);

        result.Should().NotBeNull();
        result!.Name.Should().Be("widget");
        result.Count.Should().Be(3);
    }

    [Fact]
    public async Task ToJson_ReturnsNull_WhenContentIsJsonNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("null"),
        };

        var result = await response.ToJson(TestJsonContext.Default.SamplePayload);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ToJson_ThrowsOnNonSuccessStatusCode()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("""{"name":"widget","count":3}"""),
        };
        
        await Assert.ThrowsAsync<HttpRequestException>(async () =>
            await response.ToJson(TestJsonContext.Default.SamplePayload));
    }

    [Fact]
    public async Task ToRequiredJson_ReturnsDeserializedValue()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent("""{"name":"widget","count":3}""");

        var result = await response.ToRequiredJson(TestJsonContext.Default.SamplePayload);

        result.Name.Should().Be("widget");
        result.Count.Should().Be(3);
    }

    [Fact]
    public async Task ToRequiredJson_ThrowsWhenDeserializedValueIsNull()
    {
        using var response = new HttpResponseMessage(HttpStatusCode.OK);
        response.Content = new StringContent("null");

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await response.ToRequiredJson(TestJsonContext.Default.SamplePayload));
    }

    internal sealed record SamplePayload(string Name, int Count);

    [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    [JsonSerializable(typeof(SamplePayload))]
    internal partial class TestJsonContext : JsonSerializerContext
    {
    }
}
