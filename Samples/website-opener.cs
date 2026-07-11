#!/usr/bin/env dotnet run

#:package FileBasedApp.Toolkit@0.21.0-alpha-03

using System.Net.Http.Json;
using FileBasedApp.Toolkit;
using System.Text.Json.Serialization;


var httpClient = new HttpClient();
var httpResponseMessage =  await httpClient.GetAsync(AbsoluteWebUri.Create("https://jsonplaceholder.typicode.com/todos"));
await httpResponseMessage.Content.ReadFromJsonAsync(AppContext.Default.RootObjectArray);

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

