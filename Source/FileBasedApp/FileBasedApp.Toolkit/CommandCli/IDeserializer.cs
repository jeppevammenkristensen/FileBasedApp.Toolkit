namespace FileBasedApp.Toolkit.CommandCli;

/// <summary>
/// Defines a contract for deserializing data from streams into strongly-typed objects.
/// Supports both synchronous and asynchronous deserialization operations with optional cancellation support.
/// Implementations of this interface provide specific deserialization strategies, such as JSON, XML, or custom formats.
/// </summary>
public interface IDeserializer
{
    /// <summary>
    /// Deserializes data from the specified stream into an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize to.</typeparam>
    /// <param name="stream">The stream containing the serialized data to deserialize.</param>
    /// <returns>The deserialized object of type T, or null if deserialization fails or the stream is empty.</returns>
    public T? Deserialize<T>(Stream stream);

    /// <summary>
    /// Asynchronously deserializes data from the specified stream into an object of type T with cancellation support.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize to.</typeparam>
    /// <param name="stream">The stream containing the serialized data to deserialize.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A ValueTask representing the asynchronous operation, containing the deserialized object of type T, or null if deserialization fails or the stream is empty.</returns>
    public ValueTask<T?> Deserialize<T>(Stream stream, CancellationToken cancellationToken);
}