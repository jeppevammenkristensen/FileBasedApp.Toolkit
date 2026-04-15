namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// Defines a factory interface for creating instances of simple exec runners. Implementers must provide a static factory method to initialize a new runner instance with a specified command name.
/// </summary>
/// <typeparam name="TSelf">The concrete type that implements this factory interface, enabling fluent API patterns and type-safe factory methods.</typeparam>
public interface ISimpleExecRunnerFactory<out TSelf> where TSelf : ISimpleExecRunnerFactory<TSelf>
{
    /// <summary>
    /// Initializes a new instance of a simple exec runner with the specified command name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    static abstract TSelf Init(string name);
}