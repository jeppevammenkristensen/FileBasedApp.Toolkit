using System.Net.Http;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(AbsoluteWebUriHttpExtensions))]
public class AbsoluteWebUriHttpExtensionsTest
{
    [Fact]
    public void WithBaseAddress_CorrectlySetsAddress()
    {
        var client = new HttpClient();
        var url = AbsoluteWebUri.Create("https://some.url");
        client = client.WithBaseAddress(url);
        client.BaseAddress.Should().Be(url.Uri);
    }
}