// See https://aka.ms/new-console-template for more information

using FileBasedApp.Toolkit;
using Spectre.Console;

var url = AbsoluteWebUri.Create("https://www.dr.dk")
    .AddPathSegment("jeppe")
    .AddPathSegment("ulrik")
    .AddQueryPart("jeppe","angry")
    .WithFragment("Poul");

AnsiConsole.WriteLine(url.ToString());
    
    