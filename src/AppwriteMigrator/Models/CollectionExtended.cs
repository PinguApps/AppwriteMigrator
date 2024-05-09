using Appwrite.Models;
using Newtonsoft.Json.Linq;
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

    public List<Attribute> ConvertedAttributes => Attributes.Cast<JObject>().Select(ConvertJObjectToAttribute).ToList();

    private Attribute ConvertJObjectToAttribute(JObject? jObject)
    {
        if (jObject is null)
            throw new ArgumentException(null, nameof(jObject));

        return new Attribute
        {
            Key = jObject["key"]!.ToString(),
            Type = jObject["type"]!.ToString(),
            Status = jObject["status"]!.ToString(),
            Error = jObject["error"]!.ToString(),
            Required = jObject["required"]!.ToObject<bool>(),
            Array = jObject["array"]!.ToObject<bool>(),
            Size = jObject["size"]?.ToObject<int?>(),
            Default = jObject["default"]?.ToObject<object>(),
            Min = GetTypedValue(jObject, "min"),
            Max = GetTypedValue(jObject, "max"),
            Format = jObject["format"]?.ToString(),
            Elements = jObject["elements"]?.ToObject<List<object>>(),
            RelatedCollection = jObject["relatedCollection"]?.ToString(),
            RelationType = jObject["relationType"]?.ToString(),
            TwoWay = jObject["twoWay"]?.ToObject<bool?>(),
            TwoWayKey = jObject["twoWayKey"]?.ToString(),
            OnDelete = jObject["onDelete"]?.ToString(),
            Side = jObject["side"]?.ToString()
        };
    }

    private object? GetTypedValue(JObject jObject, string propertyName)
    {
        var token = jObject[propertyName];

        if (token == null)
            return null;

        return (jObject["type"]?.ToString()) switch
        {
            "integer" => token.ToObject<int?>(),
            "double" => token.ToObject<double?>(),
            "boolean" => token.ToObject<bool?>(),
            "datetime" => token.ToObject<DateTime?>(),
            _ => token.ToObject<string?>()
        };
    }
}
