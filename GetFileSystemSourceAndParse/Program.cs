// See https://aka.ms/new-console-template for more information

using Spectre.Console.Cli;


CommandApp<RunCommand> app = new();
app.Configure(cfg => { cfg.PropagateExceptions(); });
await app.RunAsync(args);