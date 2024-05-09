using AppwriteMigrator;
using Cocona;

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();

app.AddCommands<AppwriteCommands>();

app.Run();