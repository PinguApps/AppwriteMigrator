using Appwrite;
using Appwrite.Services;
using AppwriteMigrator.Converters;
using AppwriteMigrator.Models;
using AppwriteMigrator.Utils;
using Cocona;
using System.Text.Json;

namespace AppwriteMigrator;
public class AppwriteCommands
{
    // Need to properly convert attributes...

    readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public AppwriteCommands()
    {
        _jsonSerializerOptions.Converters.Add(new JObjectConverter());
    }

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

            Console.WriteLine($"Database '{database.Name}' contains {collections.Collections.Count} collections...");

            var extendedDatabase = new DatabaseExtended(database, collections.Collections);

            schema.Add(extendedDatabase);
        }

        var json = JsonSerializer.Serialize(schema, _jsonSerializerOptions);

        await File.WriteAllTextAsync(fileName, json);

        Console.WriteLine("# Sync Complete!");
    }

    [Command(Description = "Migrates the local settings json up to the specified project.")]
    public async Task Migrate([Option('e', Description = "The API endpoint")] string endpoint,
        [Option("id", ['i'], Description = "the Project ID for the target project")] string projectId,
        [Option("key", ['k'], Description = "The API Key for the project")] string apiKey,
        [Option('f', Description = "The filename to store the schema within")] string fileName = "appwrite-schema.json")
    {
        Console.WriteLine("# Begin Migration...");

        var client = new Client()
            .SetEndpoint(endpoint)
            .SetProject(projectId)
            .SetKey(apiKey);

        var dbClient = new Databases(client);

        var json = await File.ReadAllTextAsync(fileName);

        var newSchema = JsonSerializer.Deserialize<List<DatabaseExtended>>(json, _jsonSerializerOptions);

        if (newSchema is null)
            throw new CommandExitedException("provided file did not deserialize", 1);

        Console.WriteLine($"Found {newSchema.Count} database(s) in new schema...");

        var databases = await dbClient.List();

        Console.WriteLine($"Found {databases.Databases.Count} database(s) in target...");

        List<DatabaseExtended> oldSchema = [];

        foreach (var database in databases.Databases)
        {
            var collections = await dbClient.ListCollections(database.Id);

            Console.WriteLine($"Found {collections.Collections.Count} existing collection(s) within database {database.Name}");

            var extendedDatabase = new DatabaseExtended(database, collections.Collections);

            oldSchema.Add(extendedDatabase);
        }

        Console.WriteLine("Comparing Databases...");

        // Update DB's
        foreach (var database in newSchema)
        {
            var oldDatabase = oldSchema.FirstOrDefault(x => x.Id == database.Id);

            if (oldDatabase is null)
            {
                Console.WriteLine($"Creating Database {database.Name}...");

                var result = await dbClient.Create(database.Id, database.Name, database.Enabled);

                oldSchema.Add(new DatabaseExtended(result));
            }
            else
            {
                if (oldDatabase.Name != database.Name || oldDatabase.Enabled != database.Enabled)
                {
                    Console.WriteLine($"Updating Database {database.Name}...");

                    var result = await dbClient.Update(database.Id, database.Name, database.Enabled);
                }
            }
        }

        foreach (var oldDatabase in oldSchema)
        {
            var database = newSchema.FirstOrDefault(x => x.Id == oldDatabase.Id);

            if (database is null)
            {
                Console.WriteLine($"Deleting Database {oldDatabase.Name}...");

                var result = await dbClient.Delete(oldDatabase.Id);
            }
        }

        // Update Collections
        foreach (var database in newSchema)
        {
            var oldDatabase = oldSchema.First(x => x.Id == database.Id);

            foreach (var collection in database.Collections)
            {
                var oldCollection = oldDatabase.Collections.FirstOrDefault(x => x.Id == collection.Id);

                if (oldCollection is null)
                {
                    Console.WriteLine($"Creating Collection {collection.Name}...");

                    var result = await dbClient.CreateCollection(collection.DatabaseId, collection.Id, collection.Name, collection.ConvertedPermissions, collection.DocumentSecurity, collection.Enabled);

                    oldDatabase.Collections.Add(new CollectionExtended(result));
                }
                else
                {
                    if (oldCollection.Name != collection.Name || oldCollection.Enabled != collection.Enabled ||
                        oldCollection.DocumentSecurity != collection.DocumentSecurity ||
                        ListUtils.AreStringListsDifferent(oldCollection.ConvertedPermissions, collection.ConvertedPermissions))
                    {
                        Console.WriteLine($"Updating Collection {collection.Name}...");

                        var result = await dbClient.UpdateCollection(collection.DatabaseId, collection.Id, collection.Name, collection.ConvertedPermissions, collection.DocumentSecurity, collection.Enabled);
                    }
                }
            }

            foreach (var oldCollection in oldDatabase.Collections)
            {
                var collection = database.Collections.FirstOrDefault(x => x.Id == oldCollection.Id);

                if (collection is null)
                {
                    Console.WriteLine($"Deleting Collection {oldCollection.Name}...");

                    var result = await dbClient.DeleteCollection(oldCollection.DatabaseId, oldCollection.Id);
                }
            }
        }

        Console.WriteLine("# Migration Complete!");
    }
}