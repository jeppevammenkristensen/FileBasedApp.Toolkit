// See https://aka.ms/new-console-template for more information

using System.IO.Abstractions;
using TruePath;

var fileSystem = new FileSystem();
var myPath = new AbsolutePath("/var/log/app.log");

fileSystem.File.WriteAllText(myPath, "Hello, world!");
myPath.WriteAllText("Hello world", fileSystem);
