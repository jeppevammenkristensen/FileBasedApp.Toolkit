namespace FileBasedApp.Toolkit.SimpleExec;

/// <summary>
/// A fluent builder for constructing and executing system commands via the SimpleExec library.
/// Accumulates the command name, arguments, secrets, and working directory, then executes
/// the command through an <see cref="ISimpleExecCommandWrapper"/>.
/// </summary>
public class SimpleExecRunner : BaseSimpleExecRunner<SimpleExecRunner>, ISimpleExecRunnerFactory<SimpleExecRunner>
{
    public SimpleExecRunner(string name) : base(name)
    {
    }

    /// <inheritdoc />
    public static SimpleExecRunner Init(string name)
    {
        return new SimpleExecRunner(name);
    }
}