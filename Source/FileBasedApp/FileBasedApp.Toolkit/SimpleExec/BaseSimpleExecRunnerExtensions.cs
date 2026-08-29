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
        /// Configures the runner to accept the specified error codes as valid exit codes.
        /// This allows commands to complete successfully even if their exit code matches one of the provided codes.
        /// </summary>
        /// <param name="failOnExistingErrorHandler">Throw an exception if a exit code handler has already been set</param>
        /// <param name="acceptedErrorCodes">An array of integer values representing the error codes that should be treated as successful exit codes.</param>
        /// <returns>Returns the current instance of the runner for method chaining.</returns>
        /// <remarks>
        /// This method modifies the behavior of the exit code handler to consider the specified error codes as valid.
        /// Use this method to handle cases where specific non-zero exit codes are expected and should not be treated as errors.
        /// NOTE. This will override any existing exit code handler.
        /// </remarks>
        public TSelf AcceptedErrorCodes(bool failOnExistingErrorHandler = false,params int[] acceptedErrorCodes)
        {
            return self.WithExitCodeHandler(code => acceptedErrorCodes.Contains(code), failOnExistingErrorHandler);
        }

        /// <summary>
        /// Configures the runner to treat all exit codes as valid and successful.
        /// When this handler is applied, every exit code will be accepted, regardless of its value.
        /// </summary>
        /// <param name="failOnExistingErrorHandler">Specifies whether an exception should be thrown if an existing exit code handler is already set.</param>
        /// <returns>Returns the current instance of the runner for method chaining.</returns>
        /// <remarks>
        /// Use this method when commands with any exit code should be considered successful.
        /// This overrides any pre-existing exit code handler. Ensure that this configuration aligns with your application's error-handling requirements.
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
            
            return await JsonSerializer.DeserializeAsync<T>(stream, options, cancellationToken) ?? throw new InvalidOperationException("Deserialization returned null");
        }
    }
}