// See https://aka.ms/new-console-template for more information

using System.CodeDom.Compiler;
using System.Text.RegularExpressions;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SourceGenerators;
using Spectre.Console.Cli;

Console.WriteLine("Hello, World!");

public partial class Setting : ExtendedCommandSettings
{
    [GeneratedRegex("a")]
    public partial Regex TheString();
    
    [CommandOption( "-p|--path")]
    [DirectoryPath(true, true, PredefinedRootPath.CurrentDirectory)]
    public string Path { get; set; }
}