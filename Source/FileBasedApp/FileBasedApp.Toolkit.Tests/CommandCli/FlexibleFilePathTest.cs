using System.IO;
using System.Text;
using System.Text.Unicode;
using FileBasedApp.Toolkit.CommandCli;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests.CommandCli;

[TestSubject(typeof(FlexibleFilePath))]
public class FlexibleFilePathTest
{

    [Fact]
    public void CanDeserialize()
    {
        JsonDeserializer serializer = new JsonDeserializer();

        var json = """
                   {
                        "name": "test",
                        "path": {
                            "windows": "c:\test.txt",
                            "unix": "/test.txt"
                        }
                   }
                   """;

        var result = serializer.Deserialize<TestRecord>(new MemoryStream(Encoding.UTF8.GetBytes(json), false));
        result.Should().NotBeNull();
        result.Name.Should().Be("test");
        result.Path.Should().BeEquivalentTo(new FlexibleFilePath("c:\test.txt", "/test.txt"));
    }

    internal record TestRecord(string Name, FlexibleFilePath Path);
}