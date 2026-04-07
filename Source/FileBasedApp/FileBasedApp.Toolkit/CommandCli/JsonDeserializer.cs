using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FileBasedApp.Toolkit.CommandCli;



/// <summary>
/// Provides JSON deserialization functionality for streams using System.Text.Json.
/// Implements the IDeserializer interface to deserialize JSON content into strongly-typed objects.
/// Uses web-compatible JSON serializer options by default, which can be customized by overriding the SerializerOptions method.
/// </summary>
public class JsonDeserializer : IDeserializer
{
    /// <summary>
    /// Override this method to customize the JSON serializer options used for deserialization.
    /// </summary>
    /// <returns></returns>
    protected virtual JsonSerializerOptions SerializerOptions()
    {
#if NET10_0
        return JsonSerializerOptions.Web;
#else
        return new JsonSerializerOptions(JsonSerializerDefaults.Web);
#endif
    }

    /// <summary>
    /// Deserializes a JSON stream into an object of type T.
    /// </summary>
    /// <param name="stream">The stream containing the JSON data to deserialize.</param>
    /// <typeparam name="T">The type of object to deserialize the JSON data into.</typeparam>
    /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
    public T? Deserialize<T>(Stream stream)
    {
        if (JsonTypeInfoRegistry.TryGet<T>(out var typeInfo))
        {
            return JsonSerializer.Deserialize(stream, typeInfo!);
        }

        return JsonSerializer.Deserialize<T>(stream, SerializerOptions());
    }

    /// <summary>
    /// Asynchronously deserializes a JSON stream into an object of type T.
    /// </summary>
    /// <param name="stream">The stream containing the JSON data to deserialize.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <typeparam name="T">The type of object to deserialize the JSON data into.</typeparam>
    /// <returns>A ValueTask representing the asynchronous operation, containing the deserialized object of type T, or null if deserialization fails.</returns>
    public ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken)
    {
        if (JsonTypeInfoRegistry.TryGet<T>(out var typeInfo))
        {
            return JsonSerializer.DeserializeAsync(stream, typeInfo!, cancellationToken);
        }
        
        Debug.WriteLine($"No registered JsonTypeInfo found for type {typeof(T).FullName}. Falling back to default deserialization with options.");

        return JsonSerializer.DeserializeAsync<T>(stream, SerializerOptions(), cancellationToken);
    }
}