using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Contains extension methods for <see cref="JsonSerializer"/>
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    /// Serializes the specified value to a JSON string using the provided type information.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value to serialize to JSON.</param>
    /// <param name="typeInfo">The JSON type information that describes how to serialize the value.</param>
    /// <returns>A JSON string representation of the value.</returns>
    public static string SerializeToJson<T>(this T value, JsonTypeInfo<T> typeInfo)
        => JsonSerializer.Serialize(value, typeInfo);
}
