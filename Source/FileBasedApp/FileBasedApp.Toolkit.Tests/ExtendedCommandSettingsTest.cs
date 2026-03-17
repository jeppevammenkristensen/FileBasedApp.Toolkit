using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FileBasedApp.Toolkit.CommandCli;
using FluentAssertions;
using JetBrains.Annotations;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

[TestSubject(typeof(ExtendedCommandSettings))]
public class ExtendedCommandSettingsTest : IDisposable
{
    private readonly string _tempDir;

    public ExtendedCommandSettingsTest()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"LoadSettingTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }

    private string CreateTempJsonFile(string fileName, object content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, JsonSerializer.Serialize(content, new JsonSerializerOptions(JsonSerializerDefaults.Web)));
        return filePath;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void LoadSetting_NullOrWhitespacePath_ReturnsNull(string? path)
    {
        var settings = new TestSettings();
        var result = settings.InvokeLoadSetting<SampleConfig>(path);
        result.Should().BeNull();
    }

    [Fact]
    public void LoadSetting_ValidJsonFile_DeserializesCorrectly()
    {
        var expected = new SampleConfig { Name = "TestApp", Count = 42 };
        var filePath = CreateTempJsonFile("config.json", expected);

        var settings = new TestSettings();
        var result = settings.InvokeLoadSetting<SampleConfig>(filePath);

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestApp");
        result.Count.Should().Be(42);
    }

    [Fact]
    public void LoadSetting_FileDoesNotExist_ThrowsInvalidOperationException()
    {
        var fakePath = Path.Combine(_tempDir, "nonexistent.json");

        var settings = new TestSettings();
        var act = () => settings.InvokeLoadSetting<SampleConfig>(fakePath);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void LoadSetting_WithCustomDeserializer_UsesOverriddenDeserializer()
    {
        var filePath = CreateTempJsonFile("custom.json", new SampleConfig { Name = "Original", Count = 1 });

        var settings = new TestSettingsWithCustomDeserializer();
        var result = settings.InvokeLoadSetting<SampleConfig>(filePath);

        result.Should().NotBeNull();
        result!.Name.Should().Be("CustomDeserializer");
    }

    [Fact]
    public void LoadSetting_ComplexObject_DeserializesNestedProperties()
    {
        var expected = new NestedConfig
        {
            Title = "Root",
            Inner = new SampleConfig { Name = "Child", Count = 99 }
        };
        var filePath = CreateTempJsonFile("nested.json", expected);

        var settings = new TestSettings();
        var result = settings.InvokeLoadSetting<NestedConfig>(filePath);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Root");
        result.Inner.Should().NotBeNull();
        result.Inner!.Name.Should().Be("Child");
        result.Inner.Count.Should().Be(99);
    }

    private class TestSettings : ExtendedCommandSettings
    {
        public T? InvokeLoadSetting<T>(string? path) where T : class
            => LoadSetting<T>(path);
    }

    private class TestSettingsWithCustomDeserializer : ExtendedCommandSettings
    {
        protected override IDeserializer Deserializer => new StubDeserializer();

        public T? InvokeLoadSetting<T>(string? path) where T : class
            => LoadSetting<T>(path);

        private class StubDeserializer : IDeserializer
        {
            public T? Deserialize<T>(Stream stream)
            {
                if (typeof(T) == typeof(SampleConfig))
                    return (T)(object)new SampleConfig { Name = "CustomDeserializer", Count = -1 };
                return default;
            }

            public ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken)
                => new(Deserialize<T>(stream));
        }
    }

    public class SampleConfig
    {
        public string? Name { get; set; }
        public int Count { get; set; }
    }

    public class NestedConfig
    {
        public string? Title { get; set; }
        public SampleConfig? Inner { get; set; }
    }
}
