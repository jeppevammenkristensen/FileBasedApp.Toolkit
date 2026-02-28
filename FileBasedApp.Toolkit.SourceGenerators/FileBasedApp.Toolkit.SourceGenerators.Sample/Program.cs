using System;
using FileBasedApp.Toolkit;
using FileBasedApp.Toolkit.SourceGenerators;

Console.WriteLine("Hello");

public partial class TestClass : ExtendedCommandSettings
{
    [DirectoryPath(true, true, PredefinedRootPath.CurrentDirectory)]
    public string Path { get; set; }
}