#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileBasedApp.Toolkit.SimpleExec;
using FluentAssertions;
using JetBrains.Annotations;
using NSubstitute;
using TruePath;
using Xunit;

namespace FileBasedApp.Toolkit.Tests.SimpleExec;

[TestSubject(typeof(SimpleExecRunner))]
public class SimpleExecRunnerTest
{
    private readonly ISimpleExecCommandWrapper _wrapper = Substitute.For<ISimpleExecCommandWrapper>();

    private SimpleExecRunner CreateRunner(string name = "testcmd") =>
        new SimpleExecRunner(name).WithSimpleExecWrapper(_wrapper);

    /// <summary>
    /// Extracts the arguments from the single Run or RunAsync call on the wrapper.
    /// Works for both because they share the same parameter signature.
    /// </summary>
    private RunCallArgs GetRunCallArgs()
    {
        var call = _wrapper.ReceivedCalls().Single();
        var args = call.GetArguments();
        return new RunCallArgs(
            Name: (string)args[0]!,
            Args: ((IEnumerable<string>)args[1]!).ToList(),
            WorkingDirectory: (string)args[2]!,
            ConfigureEnvironment: (Action<IDictionary<string, string?>>?)args[3],
            Secrets: ((IEnumerable<string>?)args[4])?.ToList(),
            HandleExitCode: (Func<int, bool>?)args[5],
            EchoPrefix: (string?)args[6],
            NoEcho: (bool)args[7]!,
            CancellationIgnoresProcessTree: (bool)args[8]!,
            CreateNoWindow: (bool)args[9]!);
    }

    /// <summary>
    /// Extracts the arguments from the single ReadAsync call on the wrapper.
    /// </summary>
    private ReadCallArgs GetReadCallArgs()
    {
        var call = _wrapper.ReceivedCalls().Single();
        var a = call.GetArguments();
        return new ReadCallArgs(
            Name: (string)a[0]!,
            Args: ((IEnumerable<string>)a[1]!).ToList(),
            WorkingDirectory: (string)a[2]!,
            ConfigureEnvironment: (Action<IDictionary<string, string?>>?)a[3],
            HandleExitCode: (Func<int, bool>?)a[4],
            Encoding: (Encoding?)a[5],
            StandardInput: (string?)a[6],
            CancellationIgnoresProcessTree: (bool)a[7]!);
    }

    #region Run and RunAsync pass all configured values

    public enum RunMethod
    {
        Run,
        RunAsync
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesNameAndArguments(RunMethod method)
    {
        var runner = CreateRunner("git")
            .AddArgument("status")
            .AddArgument("--short");

        await Execute(runner, method);

        var call = GetRunCallArgs();
        call.Name.Should().Be("git");
        call.Args.Should().BeEquivalentTo(["status", "--short"]);
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesWorkingDirectory(RunMethod method)
    {
        var dir = new AbsolutePath(Path.GetFullPath("/work"));
        var runner = CreateRunner().WithWorkingDirectory(dir);

        await Execute(runner, method);

        GetRunCallArgs().WorkingDirectory.Should().Be(Path.GetFullPath("/work"));
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesSecrets_WhenArgumentMarkedAsSecret(RunMethod method)
    {
        var runner = CreateRunner()
            .AddArgument("--token")
            .AddArgument("my-secret", isSecret: true);

        await Execute(runner, method);

        var call = GetRunCallArgs();
        call.Secrets.Should().Contain("my-secret");
        call.Secrets.Should().NotContain("--token");
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesConfigureEnvironment(RunMethod method)
    {
        Action<IDictionary<string, string?>> envAction = env => env["FOO"] = "bar";
        var runner = CreateRunner().WithConfigureEnvironment(envAction);

        await Execute(runner, method);

        GetRunCallArgs().ConfigureEnvironment.Should().BeSameAs(envAction);
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesCancellationIgnoresProcessTree(RunMethod method)
    {
        var runner = CreateRunner().WithCancellationIgnoresProcessTree();

        await Execute(runner, method);

        GetRunCallArgs().CancellationIgnoresProcessTree.Should().BeTrue();
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesCreateNoWindow(RunMethod method)
    {
        var runner = CreateRunner().WithCreateNoWindow(true);

        await Execute(runner, method);

        GetRunCallArgs().CreateNoWindow.Should().BeTrue();
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesExitCodeHandler(RunMethod method)
    {
        Func<int, bool> handler = code => code == 0 || code == 1;
        var runner = CreateRunner().WithExitCodeHandler(handler);

        await Execute(runner, method);

        GetRunCallArgs().HandleExitCode.Should().BeSameAs(handler);
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesEchoPrefix(RunMethod method)
    {
        var runner = CreateRunner().WithEchoPrefix("[BUILD]");

        await Execute(runner, method);

        GetRunCallArgs().EchoPrefix.Should().Be("[BUILD]");
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_PassesNoEcho(RunMethod method)
    {
        var runner = CreateRunner().WithNoEcho();

        await Execute(runner, method);

        GetRunCallArgs().NoEcho.Should().BeTrue();
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_DefaultValues_PassesExpectedDefaults(RunMethod method)
    {
        var runner = CreateRunner("cmd");

        await Execute(runner, method);

        var call = GetRunCallArgs();
        call.Name.Should().Be("cmd");
        call.Args.Should().BeEmpty();
        call.WorkingDirectory.Should().BeEmpty();
        call.Secrets.Should().BeEmpty();
        call.ConfigureEnvironment.Should().BeNull();
        call.HandleExitCode.Should().BeNull();
        call.EchoPrefix.Should().BeNull();
        call.NoEcho.Should().BeFalse();
        call.CancellationIgnoresProcessTree.Should().BeFalse();
        call.CreateNoWindow.Should().BeFalse();
    }

    private static async Task Execute(SimpleExecRunner runner, RunMethod method)
    {
        switch (method)
        {
            case RunMethod.Run:
                runner.Run();
                break;
            case RunMethod.RunAsync:
                await runner.RunAsync();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }
    }

    #endregion

    #region ReadAsync passes all configured values

    [Fact]
    public async Task ReadAsync_PassesNameAndArguments()
    {
        var runner = CreateRunner("dotnet")
            .AddArgument("build");

        await runner.ReadAsync();

        var call = GetReadCallArgs();
        call.Name.Should().Be("dotnet");
        call.Args.Should().BeEquivalentTo(["build"]);
    }

    [Fact]
    public async Task ReadAsync_PassesWorkingDirectory()
    {
        var dir = new AbsolutePath(Path.GetFullPath("/src"));
        var runner = CreateRunner().WithWorkingDirectory(dir);

        await runner.ReadAsync();

        GetReadCallArgs().WorkingDirectory.Should().Be(Path.GetFullPath("/src"));
    }

    [Fact]
    public async Task ReadAsync_PassesConfigureEnvironment()
    {
        Action<IDictionary<string, string?>> envAction = env => env["KEY"] = "val";
        var runner = CreateRunner().WithConfigureEnvironment(envAction);

        await runner.ReadAsync();

        GetReadCallArgs().ConfigureEnvironment.Should().BeSameAs(envAction);
    }

    [Fact]
    public async Task ReadAsync_PassesCancellationIgnoresProcessTree()
    {
        var runner = CreateRunner().WithCancellationIgnoresProcessTree();

        await runner.ReadAsync();

        GetReadCallArgs().CancellationIgnoresProcessTree.Should().BeTrue();
    }

    [Fact]
    public async Task ReadAsync_PassesExitCodeHandler()
    {
        Func<int, bool> handler = code => code == 0;
        var runner = CreateRunner().WithExitCodeHandler(handler);

        await runner.ReadAsync();

        GetReadCallArgs().HandleExitCode.Should().BeSameAs(handler);
    }

    [Fact]
    public async Task ReadAsync_PassesEncoding()
    {
        var runner = CreateRunner().WithEncoding(Encoding.UTF32);

        await runner.ReadAsync();

        GetReadCallArgs().Encoding.Should().Be(Encoding.UTF32);
    }

    [Fact]
    public async Task ReadAsync_PassesStandardInput()
    {
        var runner = CreateRunner().WithStandardInput("hello\n");

        await runner.ReadAsync();

        GetReadCallArgs().StandardInput.Should().Be("hello\n");
    }

    [Fact]
    public async Task ReadAsync_DefaultValues_PassesExpectedDefaults()
    {
        var runner = CreateRunner("cmd");

        await runner.ReadAsync();

        var call = GetReadCallArgs();
        call.Name.Should().Be("cmd");
        call.Args.Should().BeEmpty();
        call.WorkingDirectory.Should().BeEmpty();
        call.ConfigureEnvironment.Should().BeNull();
        call.HandleExitCode.Should().BeNull();
        call.Encoding.Should().BeNull();
        call.StandardInput.Should().BeNull();
        call.CancellationIgnoresProcessTree.Should().BeFalse();
    }

    #endregion

    #region Argument builder methods

    [Fact]
    public void AddArguments_AddsMultipleArguments()
    {
        var runner = CreateRunner()
            .AddArguments(false, "a", "b", "c");

        runner.Arguments.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public void AddArguments_WithSecret_AddsAllToSecrets()
    {
        var runner = CreateRunner()
            .AddArguments(true, "x", "y");

        runner.Secrets.Should().BeEquivalentTo(["x", "y"]);
    }

    [Fact]
    public void AddArgumentPair_AddsTwoarguments()
    {
        var runner = CreateRunner()
            .AddArgumentPair("--out", "/tmp", false);

        runner.Arguments.Should().BeEquivalentTo(["--out", "/tmp"]);
    }

    [Fact]
    public void AddArgumentPair_WithSecret_OnlyMarksValueAsSecret()
    {
        var runner = CreateRunner()
            .AddArgumentPair("--token", "secret123", isSecret: true);

        runner.Secrets.Should().Contain("secret123");
        runner.Secrets.Should().NotContain("--token secret123");
    }

    [Fact]
    public void AddSecrets_AppendsToExistingSecrets()
    {
        var runner = CreateRunner()
            .AddArgument("visible")
            .AddSecrets(false, "s1", "s2");

        runner.Secrets.Should().Contain("s1");
        runner.Secrets.Should().Contain("s2");
    }

    [Fact]
    public void AddSecrets_Strict_AllSecretsMatchArguments_AddsSecrets()
    {
        var runner = CreateRunner()
            .AddArguments("first", "second")
            .AddSecrets(true, "first", "second");

        runner.Secrets.Should().BeEquivalentTo(["first", "second"]);
    }

    [Fact]
    public void AddSecrets_Strict_SecretMatchesPartOfArgumentIgnoringCase()
    {
        var runner = CreateRunner()
            .AddArgument("prefix-SECRET-suffix")
            .AddSecrets(true, "secret");

        runner.Secrets.Should().Contain("secret");
    }

    [Fact]
    public void AddSecrets_Strict_UnmatchedSecret_ThrowsAndDoesNotAddSecrets()
    {
        var runner = CreateRunner()
            .AddArgument("present");

        var act = () => runner.AddSecrets(true, "missing");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*missing*");
        runner.Secrets.Should().BeEmpty();
    }

    [Fact]
    public void AddSecrets_Strict_MixedSecrets_ReportsOnlyUnmatchedSecrets()
    {
        var runner = CreateRunner()
            .AddArgument("contains-present-value");

        var act = () => runner.AddSecrets(true, "present", "missing-one", "missing-two");

        var exception = act.Should().Throw<InvalidOperationException>().Which;
        exception.Message.Should().Contain("missing-one");
        exception.Message.Should().Contain("missing-two");
        exception.Message.Should().NotContain("present");
        runner.Secrets.Should().BeEmpty();
    }

    [Fact]
    public void AddSecrets_NonStrict_UnmatchedSecret_IsAdded()
    {
        var runner = CreateRunner()
            .AddArgument("visible")
            .AddSecrets(false, "missing");

        runner.Secrets.Should().Contain("missing");
    }

    [Fact]
    public void AddSecrets_DuplicateSecrets_AreStoredOnce()
    {
        var runner = CreateRunner()
            .AddSecrets(false, "duplicate", "duplicate");

        runner.Secrets.Should().ContainSingle()
            .Which.Should().Be("duplicate");
    }

    [Fact]
    public void AddSecrets_NullElement_ThrowsAndDoesNotAddSecrets()
    {
        var runner = CreateRunner();

        var act = () => runner.AddSecrets(false, "valid", null!);

        act.Should().Throw<ArgumentException>();
        runner.Secrets.Should().BeEmpty();
    }

    [Fact]
    public void AddArgumentPairConditionally_ConditionTrue_AddsBothArguments()
    {
        var runner = CreateRunner()
            .AddArgumentPairConditionally("--output", "/tmp", _ => true);

        runner.Arguments.Should().BeEquivalentTo(["--output", "/tmp"]);
    }

    [Fact]
    public void AddArgumentPairConditionally_ConditionFalse_AddsNothing()
    {
        var runner = CreateRunner()
            .AddArgumentPairConditionally("--output", "/tmp", _ => false);

        runner.Arguments.Should().BeEmpty();
    }

    [Theory]
    [InlineData("value", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AddArgumentPairConditionally_ConditionReceivesValue(string? value, bool expectedAdded)
    {
        var runner = CreateRunner()
            .AddArgumentPairConditionally("--flag", value, v => !string.IsNullOrEmpty(v));

        if (expectedAdded)
        {
            runner.Arguments.Should().BeEquivalentTo(["--flag", value!]);
        }
        else
        {
            runner.Arguments.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddArgumentPairConditionally_WithSecret_MarksValueAsSecret()
    {
        var runner = CreateRunner()
            .AddArgumentPairConditionally("--token", "secret", _ => true, isSecret: true);

        runner.Arguments.Should().BeEquivalentTo(["--token", "secret"]);
        runner.Secrets.Should().Contain("secret");
        runner.Secrets.Should().NotContain("--token");
    }

    [Fact]
    public void AddArgumentPairConditionally_WithSecret_ConditionFalse_NoSecretAdded()
    {
        var runner = CreateRunner()
            .AddArgumentPairConditionally("--token", "secret", _ => false, isSecret: true);

        runner.Arguments.Should().BeEmpty();
        runner.Secrets.Should().BeEmpty();
    }

    [Theory]
    [InlineData("hello", StringNullCheck.Null, true)]
    [InlineData(null, StringNullCheck.Null, false)]
    [InlineData("hello", StringNullCheck.NullOrEmpty, true)]
    [InlineData("", StringNullCheck.NullOrEmpty, false)]
    [InlineData(null, StringNullCheck.NullOrEmpty, false)]
    [InlineData("hello", StringNullCheck.NullOrWhitespace, true)]
    [InlineData("  ", StringNullCheck.NullOrWhitespace, false)]
    [InlineData("", StringNullCheck.NullOrWhitespace, false)]
    [InlineData(null, StringNullCheck.NullOrWhitespace, false)]
    public void AddArgumentPairIfValueNotEmpty_AddsOnlyWhenValuePasses(string? value, StringNullCheck nullCheck, bool expectedAdded)
    {
        var runner = CreateRunner()
            .AddArgumentPairIfValueNotEmpty("--key", value, nullCheck);

        if (expectedAdded)
        {
            runner.Arguments.Should().BeEquivalentTo(["--key", value!]);
        }
        else
        {
            runner.Arguments.Should().BeEmpty();
        }
    }

    [Fact]
    public void AddArgumentPairIfValueNotEmpty_DefaultNullCheck_IsNull()
    {
        // Default nullCheck is StringNullCheck.Null, so empty string should still be added
        var runner = CreateRunner()
            .AddArgumentPairIfValueNotEmpty("--key", "");

        runner.Arguments.Should().BeEquivalentTo(["--key", ""]);
    }

    [Fact]
    public void AddArgumentPairIfValueNotEmpty_DefaultNullCheck_NullIsSkipped()
    {
        var runner = CreateRunner()
            .AddArgumentPairIfValueNotEmpty("--key", null);

        runner.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void AddArgumentPairIfValueNotEmpty_WithSecret_MarksValueAsSecret()
    {
        var runner = CreateRunner()
            .AddArgumentPairIfValueNotEmpty("--api-key", "my-secret", isSecret: true);

        runner.Arguments.Should().BeEquivalentTo(["--api-key", "my-secret"]);
        runner.Secrets.Should().Contain("my-secret");
    }

    [Fact]
    public void AddArgumentPairIfValueNotEmpty_WithSecret_NullValue_NoSecretAdded()
    {
        var runner = CreateRunner()
            .AddArgumentPairIfValueNotEmpty("--api-key", null, isSecret: true);

        runner.Arguments.Should().BeEmpty();
        runner.Secrets.Should().BeEmpty();
    }

    #endregion

    #region Wrapper resolution

    [Fact]
    public async Task ReadAsync_UsesExplicitCommandWrapper_WhenProvided()
    {
        var explicitWrapper = Substitute.For<ISimpleExecCommandWrapper>();
        var runner = CreateRunner("test");

        await runner.ReadAsync(commandWrapper: explicitWrapper);

        explicitWrapper.ReceivedCalls().Should().HaveCount(1);
        _wrapper.ReceivedCalls().Should().BeEmpty("the injected test wrapper should not have been called");
    }

    [Theory]
    [InlineData(RunMethod.Run)]
    [InlineData(RunMethod.RunAsync)]
    public async Task Run_UsesExplicitCommandWrapper_WhenProvided(RunMethod method)
    {
        var explicitWrapper = Substitute.For<ISimpleExecCommandWrapper>();
        var runner = CreateRunner("test");

        await Execute(runner, method, explicitWrapper);

        explicitWrapper.ReceivedCalls().Should().HaveCount(1);
        _wrapper.ReceivedCalls().Should().BeEmpty("the injected test wrapper should not have been called");
    }

    private static async Task Execute(SimpleExecRunner runner, RunMethod method,
        ISimpleExecCommandWrapper wrapper)
    {
        switch (method)
        {
            case RunMethod.Run:
                runner.Run(commandWrapper: wrapper);
                break;
            case RunMethod.RunAsync:
                await runner.RunAsync(commandWrapper: wrapper);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(method));
        }
    }

    #endregion

    #region Call argument records

    private sealed record RunCallArgs(
        string Name,
        List<string> Args,
        string WorkingDirectory,
        Action<IDictionary<string, string?>>? ConfigureEnvironment,
        IEnumerable<string>? Secrets,
        Func<int, bool>? HandleExitCode,
        string? EchoPrefix,
        bool NoEcho,
        bool CancellationIgnoresProcessTree,
        bool CreateNoWindow);

    private sealed record ReadCallArgs(
        string Name,
        List<string> Args,
        string WorkingDirectory,
        Action<IDictionary<string, string?>>? ConfigureEnvironment,
        Func<int, bool>? HandleExitCode,
        Encoding? Encoding,
        string? StandardInput,
        bool CancellationIgnoresProcessTree);

    #endregion
}