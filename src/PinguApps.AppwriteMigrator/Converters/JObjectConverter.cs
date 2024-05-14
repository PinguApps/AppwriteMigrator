using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PinguApps.AppwriteMigrator.Converters;
public class JObjectConverter : JsonConverter<JObject>
{
    public override JObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use Newtonsoft.Json to parse the JSON into a JObject
        JObject jObject = JObject.Parse(reader.GetString()!);
        return jObject;
    }

    public override void Write(Utf8JsonWriter writer, JObject value, JsonSerializerOptions options)
    {
        // Convert JObject to a JSON string using Newtonsoft.Json
        string jsonString = value.ToString();

        // Use System.Text.Json to write the JSON string as a raw value
        using (JsonDocument doc = JsonDocument.Parse(jsonString))
        {
            doc.RootElement.WriteTo(writer);
        }
    }
}
