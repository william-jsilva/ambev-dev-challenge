namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string Order { get; set; } = string.Empty; // ex: "name,-price"

    // Main filters dictionary for all types of filters
    public Dictionary<string, string> Filters { get; set; } = new();

    // Legacy properties for backward compatibility
    public IDictionary<string, string> Equality { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, (string? Min, string? Max)> Ranges { get; set; } = new Dictionary<string, (string?, string?)>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> Wildcards { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    // Helper methods
    public void AddFilter(string key, string value)
    {
        Filters[key] = value;
    }

    public void AddRangeFilter(string field, string min, string max)
    {
        if (!string.IsNullOrEmpty(min))
            Filters[$"{field}_min"] = min;
        if (!string.IsNullOrEmpty(max))
            Filters[$"{field}_max"] = max;
    }

    public void AddWildcardFilter(string field, string pattern)
    {
        Filters[field] = pattern;
    }
}
