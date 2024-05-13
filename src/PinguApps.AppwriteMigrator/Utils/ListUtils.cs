using Newtonsoft.Json.Linq;
using System.Text.Json;

namespace PinguApps.AppwriteMigrator.Utils;
public static class ListUtils
{
    public static bool AreStringListsDifferent(List<string> list1, List<string> list2)
    {
        var diff1 = list1.Except(list2).ToList();

        var diff2 = list2.Except(list1).ToList();

        return diff1.Count != 0 || diff2.Count != 0;
    }

    public static List<string> ConvertObjectListToStringList(List<object> list)
    {
        return list
            .Select(x =>
            {
                switch (x)
                {
                    case JObject jObject:
                        return jObject.ToString();
                    case JsonElement jsonElement:
                        return jsonElement.GetString()!;
                    case string str:
                        return str;
                    default:
                        throw new InvalidOperationException("Unsupported type");
                }
            })
            .ToList();
    }

    public static List<string>? ConvertNullableObjectListToStringList(List<object>? list)
    {
        if (list is null)
            return null;

        return list
            .Select(x =>
            {
                switch (x)
                {
                    case JObject jObject:
                        return jObject.ToString();
                    case JsonElement jsonElement:
                        return jsonElement.GetString()!;
                    case string str:
                        return str;
                    default:
                        throw new InvalidOperationException("Unsupported type");
                }
            })
            .ToList();
    }
}