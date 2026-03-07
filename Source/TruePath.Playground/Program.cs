// See https://aka.ms/new-console-template for more information

using System.IO.Abstractions;
using TruePath;

var fileSystem = new FileSystem();
var absolutePath = fileSystem.Path.GetTempFileAbsolute();

var myPath = new AbsolutePath("/var/log/app.log");



