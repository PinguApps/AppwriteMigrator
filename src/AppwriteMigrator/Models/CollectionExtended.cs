using Appwrite.Models;
using System.Text.Json.Serialization;

namespace AppwriteMigrator.Models;
public class CollectionExtended : Collection
{
    [JsonConstructor]
    public CollectionExtended(string id, string createdAt, string updatedAt, List<object> permissions, string databaseId, string name, bool enabled, bool documentSecurity, List<object> attributes, List<Appwrite.Models.Index> indexes) : base(id, createdAt, updatedAt, permissions, databaseId, name, enabled, documentSecurity, attributes, indexes)
    {

    }

    public CollectionExtended(Collection collection) : base(collection.Id, collection.CreatedAt, collection.UpdatedAt, collection.Permissions, collection.DatabaseId, collection.Name, collection.Enabled, collection.DocumentSecurity, collection.Attributes, collection.Indexes)
    {

    }
}
