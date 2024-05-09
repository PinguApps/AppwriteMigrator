using Cocona;

namespace AppwriteMigrator;
public class AppwriteCommands
{
    [Command(Description = "Syncs the settings from your provided project to a local json file.")]
    public async Task Sync()
    {
        Console.WriteLine("Sync!");
    }

    [Command(Description = "Migrates the local settings json up to the specified project")]
    public async Task Migrate()
    {
        Console.WriteLine("Migrate!");
    }
}