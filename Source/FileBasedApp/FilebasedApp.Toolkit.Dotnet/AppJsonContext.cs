using System.Text.Json.Serialization;

namespace FileBasedApp.Toolkit.Dotnet;

/// <summary>
/// Source-generated JSON serialization context for dotnet CLI JSON responses.
/// </summary>
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PackageRequest))]
internal partial class AppJsonContext : JsonSerializerContext
{
}