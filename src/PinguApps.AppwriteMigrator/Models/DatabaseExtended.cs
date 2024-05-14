using Appwrite.Models;
using System.Text.Json.Serialization;

namespace PinguApps.AppwriteMigrator.Models;
public class DatabaseExtended : Database
{
    [JsonConstructor]
    public DatabaseExtended(string id, string name, string createdAt, string updatedAt, bool enabled, List<CollectionExtended> collections) : base(id, name, createdAt, updatedAt, enabled)
    {
        Collections = collections;
    }

    public DatabaseExtended(string id, string name, string createdAt, string updatedAt, bool enabled, List<Collection> collections) : base(id, name, createdAt, updatedAt, enabled)
    {
        Collections = collections.Select(x => new CollectionExtended(x)).ToList();
    }

    public DatabaseExtended(string id, string name, string createdAt, string updatedAt, bool enabled) : base(id, name, createdAt, updatedAt, enabled)
    {
        Collections = [];
    }

    public DatabaseExtended(Database database, List<Collection>? collections = null) : base(database.Id, database.Name, database.CreatedAt, database.UpdatedAt, database.Enabled)
    {
        if (collections is null)
            Collections = [];
        else
            Collections = collections.Select(x => new CollectionExtended(x)).ToList();
    }

    public List<CollectionExtended> Collections { get; }
}