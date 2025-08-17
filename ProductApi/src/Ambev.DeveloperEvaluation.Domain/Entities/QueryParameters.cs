namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class QueryParameters
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string Order { get; set; } // ex: "name,-price"

    // Dicionário com filtros dinâmicos
    public Dictionary<string, string> Filters { get; set; } = new();

    // filters: equality, wildcard, ranges
    public IDictionary<string, string> Equality { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, (string? Min, string? Max)> Ranges { get; set; } = new Dictionary<string, (string?, string?)>(StringComparer.OrdinalIgnoreCase);
    public IDictionary<string, string> Wildcards { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
