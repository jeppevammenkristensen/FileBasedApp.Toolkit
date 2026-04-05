using System.Text.Json.Serialization;

namespace FilebasedApp.Toolkit.Dotnet;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(PackageRequest))]
internal partial class AppJsonContext : JsonSerializerContext
{
}