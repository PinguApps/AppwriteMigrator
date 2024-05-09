using Appwrite;
using Appwrite.Services;
using AppwriteMigrator.Models;
using Cocona;

namespace AppwriteMigrator;
public class AppwriteCommands
{
    // Add filename for json output. Default to something sensible.



    [Command(Description = "Syncs the settings from your provided project to a local json file.")]
    public async Task Sync([Option('e', Description = "The API endpoint")] string endpoint,
        [Option("id", ['i'], Description = "the Project ID for the target project")] string projectId,
        [Option("key", ['k'], Description = "The API Key for the project")] string apiKey,
        [Option('f', Description = "The filename to store the schema within")] string fileName = "appwrite-schema.json")
    {
        Console.WriteLine("# Begin Sync...");

        var client = new Client()
            .SetEndpoint(endpoint)
            .SetProject(projectId)
            .SetKey(apiKey);

        var dbClient = new Databases(client);

        var databases = await dbClient.List();

        Console.WriteLine($"Found {databases.Databases.Count} database(s)...");

        List<DatabaseExtended> schema = [];

        foreach (var database in databases.Databases)
        {
            var collections = await dbClient.ListCollections(database.Id);

            Console.WriteLine($"Database '{database.Name}' contains '{collections.Collections.Count}' collections...");

            var extended = new DatabaseExtended(database, collections.Collections);

            schema.Add(extended);
        }

        Console.WriteLine("# Sync Complete!");
    }

    [Command(Description = "Migrates the local settings json up to the specified project.")]
    public async Task Migrate()
    {
        Console.WriteLine("Migrate!");
    }
}