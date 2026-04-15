using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FileBasedApp.Toolkit;

/// <summary>
/// Provides extension methods for <see cref="HttpResponseMessage"/>.
/// </summary>
public static class HttpResponseMessageExtensions
{
    extension(HttpResponseMessage response)
    {
        /// <summary>
        /// Tries to deserialize the response content as JSON into the specified type.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<T?> FromJson<T>(JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
        {
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            return await JsonSerializer.DeserializeAsync(stream, typeInfo, cancellationToken);
        }

        /// <summary>
        /// Tries to deserialize the response content as JSON and throw an exception if null
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<T> FromRequiredJson<T>(JsonTypeInfo<T> typeInfo, CancellationToken cancellationToken = default)
        {
            return (await response.FromJson(typeInfo, cancellationToken)) ?? throw new InvalidOperationException("Deserialization returned null");
        }
    }
}