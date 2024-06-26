﻿using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PinguApps.AppwriteMigrator.Models;
public record Attribute
{
    public string Key { get; set; } = default!;

    public string Type { get; set; } = default!;

    public string Status { get; set; } = default!;

    public string Error { get; set; } = default!;

    public bool Required { get; set; }

    public bool Array { get; set; }

    public long? Size { get; set; }

    public object? Default { get; set; }

    public object? Min { get; set; }

    public object? Max { get; set; }

    public string? Format { get; set; }

    public ElementList? Elements { get; set; }

    public string? RelatedCollection { get; set; }

    public string? RelationType { get; set; }

    public bool? TwoWay { get; set; }

    public string? TwoWayKey { get; set; }

    public string? OnDelete { get; set; }

    public string? Side { get; set; }

    public bool? Encrypt { get; set; }

    [JsonIgnore]
    public List<string>? ConvertedElements => Elements?
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
                    throw new InvalidOperationException("Unsupported element");
            }
        })
        .ToList();
}

public class ElementList : List<object>
{
    public ElementList() : base()
    {

    }

    public ElementList(IEnumerable<object> items) : base(items)
    {
    }

    public override bool Equals(object? obj)
    {
        return obj is ElementList other && this.SequenceEqual(other);
    }

    public override int GetHashCode()
    {
        return this.Aggregate(17, (acc, val) => acc * 23 + (val?.GetHashCode() ?? 0));
    }
}