using Appwrite.Models;
using Newtonsoft.Json.Linq;
using PinguApps.AppwriteMigrator.Utils;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PinguApps.AppwriteMigrator.Models;
public class CollectionExtended : Collection
{
    [JsonConstructor]
    public CollectionExtended(string id, string createdAt, string updatedAt, List<object> permissions, string databaseId, string name, bool enabled, bool documentSecurity, List<object> attributes, List<Appwrite.Models.Index> indexes) : base(id, createdAt, updatedAt, permissions, databaseId, name, enabled, documentSecurity, attributes, indexes)
    {

    }

    public CollectionExtended(Collection collection) : base(collection.Id, collection.CreatedAt, collection.UpdatedAt, collection.Permissions, collection.DatabaseId, collection.Name, collection.Enabled, collection.DocumentSecurity, collection.Attributes, collection.Indexes)
    {

    }

    [JsonIgnore]
    public List<Attribute> ConvertedAttributes => Attributes.
        Select(x =>
        {
            switch (x)
            {
                case JObject jObject:
                    return ConvertJObjectToAttribute(jObject);
                case JsonElement jsonElement:
                    return ConvertJsonElementToAttribute(jsonElement);
                default:
                    throw new InvalidOperationException("Unsupported attribute type");
            }
        })
        .ToList();

    [JsonIgnore]
    public List<string> ConvertedPermissions => ListUtils.ConvertObjectListToStringList(Permissions);

    private Attribute ConvertJObjectToAttribute(JObject jObject)
    {
        return new Attribute
        {
            Key = jObject["key"]!.ToString(),
            Type = jObject["type"]!.ToString(),
            Status = jObject["status"]!.ToString(),
            Error = jObject["error"]!.ToString(),
            Required = jObject["required"]!.ToObject<bool>(),
            Array = jObject["array"]!.ToObject<bool>(),
            Size = jObject["size"]?.ToObject<int?>(),
            Default = GetTypedValue(jObject, "default"),
            Min = GetTypedValue(jObject, "min"),
            Max = GetTypedValue(jObject, "max"),
            Format = jObject["format"]?.ToString(),
            Elements = jObject["elements"]?.ToObject<ElementList?>(),
            RelatedCollection = jObject["relatedCollection"]?.ToString(),
            RelationType = jObject["relationType"]?.ToString(),
            TwoWay = jObject["twoWay"]?.ToObject<bool?>(),
            TwoWayKey = jObject["twoWayKey"]?.ToString(),
            OnDelete = jObject["onDelete"]?.ToString(),
            Side = jObject["side"]?.ToString()
        };
    }

    private Attribute ConvertJsonElementToAttribute(JsonElement jsonElement)
    {
        return new Attribute
        {
            Key = jsonElement.GetProperty("key").GetString()!,
            Type = jsonElement.GetProperty("type").GetString()!,
            Status = jsonElement.GetProperty("status").GetString()!,
            Error = jsonElement.GetProperty("error").GetString()!,
            Required = jsonElement.GetProperty("required").GetBoolean(),
            Array = jsonElement.GetProperty("array").GetBoolean(),
            Size = jsonElement.TryGetProperty("size", out JsonElement sizeElement) ? sizeElement.TryGetInt32(out int size) ? size : null : null,
            Default = GetTypedValue(jsonElement, "default"),
            Min = GetTypedValue(jsonElement, "min"),
            Max = GetTypedValue(jsonElement, "max"),
            Format = jsonElement.TryGetProperty("format", out JsonElement formatElement) ? formatElement.GetString() : null,
            Elements = GetJsonArrayToTypedList(jsonElement, "elements"),
            RelatedCollection = jsonElement.TryGetProperty("relatedCollection", out JsonElement relatedCollectionElement) ? relatedCollectionElement.GetString() : null,
            RelationType = jsonElement.TryGetProperty("relationType", out JsonElement relationTypeElement) ? relationTypeElement.GetString() : null,
            TwoWay = jsonElement.TryGetProperty("twoWay", out JsonElement twoWayElement) ? twoWayElement.GetBoolean() : null,
            TwoWayKey = jsonElement.TryGetProperty("twoWayKey", out JsonElement twoWayKeyElement) ? twoWayKeyElement.GetString() : null,
            OnDelete = jsonElement.TryGetProperty("onDelete", out JsonElement onDeleteElement) ? onDeleteElement.GetString() : null,
            Side = jsonElement.TryGetProperty("side", out JsonElement sideElement) ? sideElement.GetString() : null
        };
    }

    private object? GetTypedValue(JObject jObject, string propertyName)
    {
        var token = jObject[propertyName];

        if (token == null)
            return null;

        var ggg = token.Type;

        return (jObject["type"]?.ToString()) switch
        {
            "integer" => token.ToObject<long?>(),
            "double" => token.ToObject<double?>(),
            "boolean" => token.ToObject<bool?>(),
            "datetime" => token.Type == JTokenType.Date ? token.ToObject<DateTime>().ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) : token.ToObject<string?>(),
            _ => token.ToObject<string?>()
        };
    }

    private object? GetTypedValue(JsonElement jsonElement, string propertyName)
    {
        if (!jsonElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return null;

        if (propertyElement.ValueKind == JsonValueKind.Null)
            return null;

        var type = jsonElement.GetProperty("type").GetString();

        return type switch
        {
            "integer" => propertyElement.TryGetInt64(out long val) ? val : null,
            "double" => propertyElement.TryGetDouble(out double val) ? val : null,
            "boolean" => propertyElement.GetBoolean(),
            "datetime" => propertyElement.GetString(),
            _ => propertyElement.GetString()
        };
    }

    private ElementList? GetJsonArrayToTypedList(JsonElement jsonElement, string propertyName)
    {
        if (!jsonElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            return null;

        var type = jsonElement.GetProperty("type").GetString();

        var list = new ElementList();

        foreach (var element in propertyElement.EnumerateArray())
        {
            switch (type)
            {
                case "integer":
                    if (element.TryGetInt32(out int intVal))
                        list.Add(intVal);
                    break;
                case "double":
                    if (element.TryGetDouble(out double doubleVal))
                        list.Add(doubleVal);
                    break;
                case "boolean":
                    list.Add(element.GetBoolean());
                    break;
                case "datetime":
                    var dateVal = element.GetString();
                    if (dateVal is not null)
                        list.Add(dateVal);
                    break;
                default:
                    var strVal = element.GetString();
                    if (strVal is not null)
                        list.Add(strVal);
                    break;
            }
        }

        return list;
    }
}
