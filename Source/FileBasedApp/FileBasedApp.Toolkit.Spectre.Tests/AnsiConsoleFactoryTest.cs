using System;
using System.IO;
using FluentAssertions;
using JetBrains.Annotations;
using Spectre.Console;
using Xunit;

namespace FileBasedApp.Toolkit.Tests;

/// <summary>
/// Keeps process-wide console redirection isolated from other tests.
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public sealed class ConsoleCollection
{
}

/// <summary>
/// Verifies that diagnostic consoles never pollute standard output.
/// </summary>
[Collection("Sequential")]
[TestSubject(typeof(AnsiConsoleFactory))]
public class AnsiConsoleFactoryTest
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Factory_RoutesOutputWithoutWritingToStdout(bool quiet)
    {
        var originalOutput = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();

        try
        {
            Console.SetOut(output);
            Console.SetError(error);
            var console = quiet ? AnsiConsoleFactory.Quiet() : AnsiConsoleFactory.Stderr();

            console.WriteLine("diagnostic output");
            console.MarkupLine("[red]diagnostic markup[/]");

            output.ToString().Should().BeEmpty();
            if (quiet)
            {
                error.ToString().Should().BeEmpty();
            }
            else
            {
                error.ToString().Should().Contain("diagnostic output").And.Contain("diagnostic markup");
            }
        }
        finally
        {
            Console.SetOut(originalOutput);
            Console.SetError(originalError);
        }
    }
}
