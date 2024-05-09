namespace AppwriteMigrator.Models;
public record Attribute
{
    public string Key { get; set; } = default!;

    public string Type { get; set; } = default!;

    public string Status { get; set; } = default!;

    public string Error { get; set; } = default!;

    public bool Required { get; set; }

    public bool Array { get; set; }

    public int? Size { get; set; }

    public object? Default { get; set; }

    public object? Min { get; set; }

    public object? Max { get; set; }

    public string? Format { get; set; }

    public List<object>? Elements { get; set; }

    public string? RelatedCollection { get; set; }

    public string? RelationType { get; set; }

    public bool? TwoWay { get; set; }

    public string? TwoWayKey { get; set; }

    public string? OnDelete { get; set; }

    public string? Side { get; set; }
}