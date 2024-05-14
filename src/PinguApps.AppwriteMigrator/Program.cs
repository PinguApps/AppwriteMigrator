using Cocona;
using PinguApps.AppwriteMigrator;

var builder = CoconaApp.CreateBuilder();

var app = builder.Build();

app.AddCommands<AppwriteCommands>();

app.Run();