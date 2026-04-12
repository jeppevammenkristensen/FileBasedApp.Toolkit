// See https://aka.ms/new-console-template for more information

using FileBasedApp.Toolkit;
using Spectre.Console;



var url = AbsoluteWebUri.Create("https://www.dr.dk")
    / UriPathSegment.From("first") 
    / UriPathSegment.From("second") 
    / UriQueryString.From("a=1&b=2")
    / UriFragment.From("Fragment");

AnsiConsole.WriteLine(url.ToString());
    
    