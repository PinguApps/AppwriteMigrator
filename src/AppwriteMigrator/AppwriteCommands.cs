using Appwrite;
using Appwrite.Services;
using Cocona;

namespace AppwriteMigrator;
public class AppwriteCommands
{
    [Command(Description = "Syncs the settings from your provided project to a local json file.")]
    public async Task Sync([Option('e', Description = "The API endpoint")] string endpoint,
        [Option("id", ['i'], Description = "the Project ID for the target project")] string projectId,
        [Option("key", ['k'], Description = "The API Key for the project")] string apiKey)
    {
        Console.WriteLine("# Begin Sync...");

        var client = new Client()
            .SetEndpoint(endpoint)
            .SetProject(projectId)
            .SetKey(apiKey);

        var databases = new Databases(client);

        var dbList = await databases.List();
    }

    [Command(Description = "Migrates the local settings json up to the specified project.")]
    public async Task Migrate()
    {
        Console.WriteLine("Migrate!");
    }
}