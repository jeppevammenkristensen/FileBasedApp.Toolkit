using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using JetBrains.Annotations;

namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// Extension methods for <see cref="BaseSimpleExecRunner{TSelf}"/> providing additional fluent operations such as JSON deserialization of command output.
/// </summary>
[PublicAPI]
public static class BaseSimpleExecRunnerExtensions
{
    extension<TSelf>(BaseSimpleExecRunner<TSelf> self) where TSelf : BaseSimpleExecRunner<TSelf>
    {
        /// <summary>
        /// Accepts the specified non-zero exit codes so the command can complete without throwing.
        /// </summary>
        /// <param name="acceptedErrorCodes">The exit codes to accept in addition to the default successful exit code, <c>0</c>.</param>
        /// <param name="failOnExistingErrorHandler">
        /// <see langword="true"/> to throw when an exit-code handler is already configured;
        /// otherwise, a non-empty array replaces the existing handler.
        /// </param>
        /// <returns>The current runner instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a handler is already configured and <paramref name="failOnExistingErrorHandler"/> is <see langword="true"/>.
        /// </exception>
        /// <remarks>
        /// SimpleExec handles exit code <c>0</c> independently, so it does not need to be included.
        /// A listed non-zero code is handled without throwing; any other non-zero code retains SimpleExec's failure behavior.
        /// Passing <see langword="null"/> or an empty array is a no-op and preserves the current handler.
        /// Repeated calls with non-empty arrays replace rather than combine the accepted codes.
        /// Use <c>WithExitCodeHandler</c> for advanced callback-based handling.
        /// </remarks>
        public TSelf WithAcceptedErrorCodes(int[] acceptedErrorCodes, bool failOnExistingErrorHandler = false)
        {
            if (acceptedErrorCodes is null or { Length: 0 })
            {
                return (TSelf)self;
            }
            
            return self.WithExitCodeHandler(code => acceptedErrorCodes.Contains(code), failOnExistingErrorHandler);
        }

        /// <summary>
        /// Handles every exit code without applying SimpleExec's default non-zero-exit-code failure behavior.
        /// </summary>
        /// <param name="failOnExistingErrorHandler">
        /// <see langword="true"/> to throw when an exit-code handler is already configured;
        /// otherwise, replace the existing handler.
        /// </param>
        /// <returns>The current runner instance for method chaining.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a handler is already configured and <paramref name="failOnExistingErrorHandler"/> is <see langword="true"/>.
        /// </exception>
        /// <remarks>
        /// Use this only when every non-zero exit code is an expected result. By default, this replaces any previously configured handler.
        /// </remarks>
        public TSelf HandlesAllErrorCodes(bool failOnExistingErrorHandler = false)
        {
            return self.WithExitCodeHandler(_ => true, failOnExistingErrorHandler);
        }
        
        /// <summary>
        /// Runs the command and returns the standard output and standard error as a tuple. This is here
        /// as a convenience method to help make you more likely to use this if you use an mcp as this call
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        /// <remarks>This is basically just a wrapper for calling ReadAsync and is meant to guide you in the right direction
        /// if you are using a mcp as it doesn't use the Console streams in anyway</remarks>
        public Task<(string StandardOutput, string StandardError)> RunMcpSafe(CancellationToken token = default)
        {
            return self.ReadAsync(token: token);
        }
        
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
            
            return await JsonSerializer.DeserializeAsync(stream, options, cancellationToken) ?? throw new InvalidOperationException("Deserialization returned null");
        }
    }
}