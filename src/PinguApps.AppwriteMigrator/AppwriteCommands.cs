using Appwrite;
using Appwrite.Enums;
using Appwrite.Services;
using Cocona;
using PinguApps.AppwriteMigrator.Converters;
using PinguApps.AppwriteMigrator.Models;
using PinguApps.AppwriteMigrator.Utils;
using System.Text.Json;

namespace PinguApps.AppwriteMigrator;
public class AppwriteCommands
{
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
        [Option('f', Description = "The filename containing the schema definition")] string fileName = "appwrite-schema.json")
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

            Console.WriteLine($"Found {collections.Collections.Count} existing collection(s) within database {database.Name}...");

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

        Console.WriteLine("Comparing Collections...");
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

        Console.WriteLine("Comparing Attributes...");
        // Update Attributes
        foreach (var database in newSchema)
        {
            var oldDatabase = oldSchema.First(x => x.Id == database.Id);

            foreach (var collection in database.Collections)
            {
                var oldCollection = oldDatabase.Collections.First(x => x.Id == collection.Id);

                foreach (var attribute in collection.ConvertedAttributes)
                {
                    var oldAttribute = oldCollection.ConvertedAttributes.FirstOrDefault(x => x.Key == attribute.Key);

                    if (oldAttribute is null)
                    {
                        Console.WriteLine($"Creating Attribute {attribute.Key}...");

                        await CreateAttribute(dbClient, attribute, collection.DatabaseId, collection.Id);
                    }
                    else
                    {
                        if (attribute != oldAttribute)
                        {
                            Console.WriteLine($"Updating Attribute {attribute.Key}...");

                            await UpdateAttribute(dbClient, oldAttribute, attribute, collection.DatabaseId, collection.Id);
                        }
                    }
                }

                foreach (var oldAttribute in oldCollection.ConvertedAttributes)
                {
                    var attribute = collection.ConvertedAttributes.FirstOrDefault(x => x.Key == oldAttribute.Key);

                    if (attribute is null)
                    {
                        Console.WriteLine($"Deleting Attribute {oldAttribute.Key}...");

                        await DeleteAttribute(dbClient, oldAttribute, collection.DatabaseId, collection.Id);
                    }
                }
            }
        }

        Console.WriteLine("Rebuilding current schema...");
        // Rebuild oldSchema before working on indexes
        oldSchema = [];

        databases = await dbClient.List();

        Console.WriteLine($"Found {databases.Databases.Count} database(s) in target...");

        foreach (var database in databases.Databases)
        {
            var collections = await dbClient.ListCollections(database.Id);

            Console.WriteLine($"Found {collections.Collections.Count} existing collection(s) within database {database.Name}...");

            var extendedDatabase = new DatabaseExtended(database, collections.Collections);

            oldSchema.Add(extendedDatabase);
        }

        Console.WriteLine("Comparing Indexes...");
        // Update Indexes
        foreach (var database in newSchema)
        {
            var oldDatabase = oldSchema.First(x => x.Id == database.Id);

            foreach (var collection in database.Collections)
            {
                var oldCollection = oldDatabase.Collections.First(x => x.Id == collection.Id);

                foreach (var index in collection.Indexes)
                {
                    var oldIndex = oldCollection.Indexes.FirstOrDefault(x => x.Key == index.Key);

                    if (oldIndex is null)
                    {
                        Console.WriteLine($"Creating Index {index.Key}...");

                        var result = await dbClient.CreateIndex(collection.DatabaseId, collection.Id, index.Key,
                            new IndexType(index.Type), ListUtils.ConvertObjectListToStringList(index.Attributes),
                            ListUtils.ConvertNullableObjectListToStringList(index.Orders));

                        oldCollection.Indexes.Add(result);
                    }
                    else
                    {
                        if (oldIndex.Type != index.Type ||
                            !ListUtils.ConvertObjectListToStringList(oldIndex.Attributes).SequenceEqual(ListUtils.ConvertObjectListToStringList(index.Attributes)) ||
                            (oldIndex.Orders is not null && index.Orders is null) ||
                            (oldIndex.Orders is null && index.Orders is not null) ||
                            (oldIndex.Orders is not null && index.Orders is not null && !ListUtils.ConvertObjectListToStringList(oldIndex.Orders).SequenceEqual(ListUtils.ConvertObjectListToStringList(index.Orders))))
                        {
                            Console.WriteLine($"Updating Index {index.Key}...");

                            await dbClient.DeleteIndex(collection.DatabaseId, collection.Id, oldIndex.Key);

                            oldCollection.Indexes.Remove(oldIndex);

                            var result = await dbClient.CreateIndex(collection.DatabaseId, collection.Id, index.Key,
                                new IndexType(index.Type), ListUtils.ConvertObjectListToStringList(index.Attributes),
                                ListUtils.ConvertNullableObjectListToStringList(index.Orders));

                            oldCollection.Indexes.Add(result);
                        }
                    }
                }

                foreach (var oldIndex in oldCollection.Indexes)
                {
                    var index = collection.Indexes.FirstOrDefault(x => x.Key == oldIndex.Key);

                    if (index is null)
                    {
                        Console.WriteLine($"Deleting Index {oldIndex.Key}...");

                        oldCollection.Indexes.Remove(oldIndex);

                        oldCollection.Indexes.Remove(oldIndex);
                    }
                }
            }
        }

        Console.WriteLine("# Migration Complete!");
    }

    private static async Task CreateAttribute(Databases dbClient, Models.Attribute attribute, string databaseId, string collectionId)
    {
        switch (attribute.Type)
        {
            case "string":
                switch (attribute.Format)
                {
                    case null:
                        await dbClient.CreateStringAttribute(databaseId, collectionId, attribute.Key, attribute.Size!.Value,
                            attribute.Required, attribute.Default is string defaultStringVal ? defaultStringVal : null, attribute.Array,
                            attribute.Encrypt);
                        break;
                    case "email":
                        await dbClient.CreateEmailAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                            attribute.Default is string defaultEmailVal ? defaultEmailVal : null, attribute.Array);
                        break;
                    case "ip":
                        await dbClient.CreateIpAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                            attribute.Default is string defaultIpVal ? defaultIpVal : null, attribute.Array);
                        break;
                    case "url":
                        await dbClient.CreateUrlAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                            attribute.Default is string defaultUrlVal ? defaultUrlVal : null, attribute.Array);
                        break;
                    case "enum":
                        await dbClient.CreateEnumAttribute(databaseId, collectionId, attribute.Key, attribute.ConvertedElements ?? [],
                            attribute.Required, attribute.Default is string defaultEnumVal ? defaultEnumVal : null, attribute.Array);
                        break;
                    default:
                        throw new CommandExitedException("Unknown attribute type and format", 1);
                }
                break;
            case "integer":
                await dbClient.CreateIntegerAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                    attribute.Min is long minIntVal ? minIntVal : null, attribute.Max is long maxIntVal ? maxIntVal : null,
                    attribute.Default is long defaultIntVal ? defaultIntVal : null, attribute.Array);
                break;
            case "double":
                await dbClient.CreateFloatAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                    attribute.Min is double minFloatVal ? minFloatVal : null, attribute.Max is double maxFloatVal ? maxFloatVal : null,
                    attribute.Default is double defaultDoubleVal ? defaultDoubleVal : null, attribute.Array);
                break;
            case "boolean":
                await dbClient.CreateBooleanAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                    attribute.Default is bool defaultBoolValue ? defaultBoolValue : null, attribute.Array);
                break;
            case "datetime":
                await dbClient.CreateDatetimeAttribute(databaseId, collectionId, attribute.Key, attribute.Required,
                    attribute.Default is string defaultStringValue ? defaultStringValue : null, attribute.Array);
                break;
            case "relationship":
                if (attribute.Side != "child")
                {
                    await dbClient.CreateRelationshipAttribute(databaseId, collectionId, attribute.RelatedCollection!,
                        new RelationshipType(attribute.RelationType!), attribute.TwoWay, attribute.Key, attribute.TwoWayKey,
                        new RelationMutate(attribute.OnDelete!));
                }
                break;
            default:
                throw new CommandExitedException("Unknown attribute type", 1);
        }
    }

    private static Task DeleteAttribute(Databases dbClient, Models.Attribute attribute, string databaseId, string collectionId)
    {
        return dbClient.DeleteAttribute(databaseId, collectionId, attribute.Key);
    }

    private static async Task UpdateAttribute(Databases dbClient, Models.Attribute oldAttribute, Models.Attribute newAttribute, string databaseId, string collectionId)
    {
        if (oldAttribute.Type != newAttribute.Type ||
            oldAttribute.Size != newAttribute.Size ||
            oldAttribute.Array != newAttribute.Array ||
            oldAttribute.Format != newAttribute.Format ||
            oldAttribute.RelatedCollection != newAttribute.RelatedCollection ||
            oldAttribute.RelationType != newAttribute.RelationType ||
            oldAttribute.TwoWay != newAttribute.TwoWay ||
            oldAttribute.TwoWayKey != newAttribute.TwoWayKey ||
            oldAttribute.Side != newAttribute.Side)
        {
            // This isn't a change we can modify, so we delete and recreate.
            // This will cause a loss of data, data will not be preserved through this change.

            await DeleteAttribute(dbClient, oldAttribute, databaseId, collectionId);
            await CreateAttribute(dbClient, newAttribute, databaseId, collectionId);
        }
        else
        {
            switch (newAttribute.Type)
            {
                case "string":
                    switch (newAttribute.Format)
                    {
                        case null:
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                            await dbClient.UpdateStringAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                                newAttribute.Default is string defaultStringVal ? defaultStringVal : null);
#pragma warning restore CS8604 // Possible null reference argument.
                            break;
                        case "email":
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                            await dbClient.UpdateEmailAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                                newAttribute.Default is string defaultEmailVal ? defaultEmailVal : null);
#pragma warning restore CS8604 // Possible null reference argument.
                            break;
                        case "ip":
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                            await dbClient.UpdateIpAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                                newAttribute.Default is string defaultIpVal ? defaultIpVal : null);
#pragma warning restore CS8604 // Possible null reference argument.
                            break;
                        case "url":
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                            await dbClient.UpdateUrlAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                                newAttribute.Default is string defaultUrlVal ? defaultUrlVal : null);
#pragma warning restore CS8604 // Possible null reference argument.
                            break;
                        case "enum":
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                            await dbClient.UpdateEnumAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.ConvertedElements ?? [],
                                newAttribute.Required, newAttribute.Default is string defaultEnumVal ? defaultEnumVal : null);
#pragma warning restore CS8604 // Possible null reference argument.
                            break;
                        default:
                            throw new CommandExitedException("Unknown attribute type and format", 1);
                    }
                    break;

                // TODO - Client currently doesn't allow null values to be passed for min, max, default.
                // We will need to replace our own default values here with nullls once this is fixed.
                // See issue #38

                case "integer":
                    await dbClient.UpdateIntegerAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                    newAttribute.Min is long minIntVal ? minIntVal : int.MinValue, newAttribute.Max is long maxIntVal ? maxIntVal : int.MaxValue,
                        newAttribute.Default is long defaultIntVal ? defaultIntVal : 0);
                    break;
                case "double":
                    await dbClient.UpdateFloatAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                    newAttribute.Min is double minFloatVal ? minFloatVal : int.MinValue, newAttribute.Max is double maxFloatVal ? maxFloatVal : int.MaxValue,
                        newAttribute.Default is double defaultDoubleVal ? defaultDoubleVal : 0);
                    break;
                case "boolean":
                    await dbClient.UpdateBooleanAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                        newAttribute.Default is bool defaultBoolValue ? defaultBoolValue : false);
                    break;
                case "datetime":
#pragma warning disable CS8604 // Possible null reference argument. SDK incorrectly expects non nullable.
                    await dbClient.UpdateDatetimeAttribute(databaseId, collectionId, newAttribute.Key, newAttribute.Required,
                        newAttribute.Default is string defaultStringValue ? defaultStringValue : null);
#pragma warning restore CS8604 // Possible null reference argument.
                    break;
                case "relationship":
                    if (newAttribute.Side != "child")
                    {
                        await dbClient.UpdateRelationshipAttribute(databaseId, collectionId, newAttribute.Key,
                            new RelationMutate(newAttribute.OnDelete!));
                    }
                    break;
                default:
                    throw new CommandExitedException("Unknown attribute type", 1);
            }
        }
    }
}