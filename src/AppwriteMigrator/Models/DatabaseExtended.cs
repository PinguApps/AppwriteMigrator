using Appwrite.Models;

namespace AppwriteMigrator.Models;
public class DatabaseExtended : Database
{
    public DatabaseExtended(string id, string name, string createdAt, string updatedAt, bool enabled) : base(id, name, createdAt, updatedAt, enabled)
    {

    }

    public DatabaseExtended(Database database, List<Collection> collections) : base(database.Id, database.Name, database.CreatedAt, database.UpdatedAt, database.Enabled)
    {
        Collections = collections;
    }

    public List<Collection> Collections { get; } = [];
}