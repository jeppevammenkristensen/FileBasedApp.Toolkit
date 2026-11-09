using FileBasedApp.Toolkit.SimpleExec;
using FluentAssertions;
using Xunit;

namespace FileBasedApp.Toolkit.Tests.SimpleExec;

/// <summary>
/// Guards the immutable default wrapper without replacing process-wide state.
/// </summary>
public class SimpleExecCommandTest
{
    [Fact]
    public void Instance_IsStableAndReadOnly()
    {
        SimpleExecCommand.Instance.Should().BeOfType<SimpleExecCommand>();
        SimpleExecCommand.Instance.Should().BeSameAs(SimpleExecCommand.Instance);
        typeof(SimpleExecCommand).GetProperty(nameof(SimpleExecCommand.Instance))!
            .CanWrite.Should().BeFalse();
    }

    [Fact]
    public void Command_DoesNotImplementCoreStaticValueSetter()
    {
        typeof(SimpleExecCommand).GetInterfaces().Should()
            .NotContain(type => type.Name.StartsWith("IStaticValueSetter"));
    }
}
