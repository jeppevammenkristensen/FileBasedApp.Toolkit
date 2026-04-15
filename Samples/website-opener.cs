#!/usr/bin/env dotnet run

#:package FileBasedApp.Toolkit@0.20.0-alpha-04

using FileBasedApp.Toolkit;
using Spectre.Console;
using System.Text.Json;
using System.Text.Json.Serialization;


var httpResponseMessage = await AbsoluteWebUri.Create("https://jsonplaceholder.typicode.com/todos").GetAsync(new HttpClient());
await httpResponseMessage.ToRequiredJson(AppContext.Default.RootObjectArray);

public class RootObject
{
    public int UserId { get; set; }
    public int Id { get; set; }
    public string Title { get; set; }
    public bool Completed { get; set; }
}


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(RootObject[]))]
public partial class AppContext : JsonSerializerContext
{

}

