using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// Extension methods for <see cref="BaseSimpleExecRunner{TSelf}"/> providing additional fluent operations such as JSON deserialization of command output.
/// </summary>
public static class BaseSimpleExecRunnerExtensions
{
    extension<TSelf>(BaseSimpleExecRunner<TSelf> self) where TSelf : BaseSimpleExecRunner<TSelf>
    {
        /// <summary>
        /// Executes the command, reads the standard output, and deserializes the JSON output into an object of type T.
        /// </summary>
        /// <param name="options">The JSON type information used for deserialization.</param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <typeparam name="T">The type to deserialize the JSON output into.</typeparam>
        /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object of type T.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the command produces standard error output or when deserialization returns null.</exception>
        /// <remarks>
        /// You declare a <paramref name="options"/> like this
        /// <code>
        /// [JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
        /// [JsonSerializable(typeof(SomeDataStructure))]
        /// internal partial class AppJsonContext : JsonSerializerContext
        /// {
        /// }
        /// </code>
        ///
        /// and then pass it as AppJsonContext.Default.SomeDataStructure. This ensures that reflection is not used to deserialize
        /// </remarks>
        public async Task<T> ReadAndParseJson<T>(JsonTypeInfo<T> options,
            CancellationToken cancellationToken = default)
        {
            var (standardOutput, standardError) = await self.ReadAsync(token: cancellationToken);
            if (!standardError.IsNullOrWhitespace())
            {
                throw new InvalidOperationException($"Command failed with error: {standardError}");
            }

            var bytes = Encoding.UTF8.GetBytes(standardOutput);
            using var stream = new MemoryStream(bytes);
            
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken) ?? throw new InvalidOperationException("Deserialization returned null");
        }
    }
}