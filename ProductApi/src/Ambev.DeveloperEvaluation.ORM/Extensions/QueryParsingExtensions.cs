using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Ambev.DeveloperEvaluation.ORM.Extensions;

public static class QueryParsingExtensions
{
    public static QueryParameters ParseFrom(HttpRequest request, int defaultPage, int defaultSize, int maxSize)
    {
        var qp = new QueryParameters
        {
            Page = TryInt(request.Query["_page"], defaultPage),
            Size = Math.Min(TryInt(request.Query["_size"], defaultSize), maxSize),
            Order = request.Query["_order"].FirstOrDefault() ?? string.Empty
        };

        // Track range filters to combine min/max values
        var rangeFilters = new Dictionary<string, (string? Min, string? Max)>();

        // Process all query parameters
        foreach (var kv in request.Query)
        {
            var key = kv.Key;
            var value = kv.Value.ToString();

            // Skip reserved parameters
            if (key.StartsWith("_")) continue;

            // Handle range filters (_min and _max)
            var minMatch = Regex.Match(key, @"^_min(.+)$", RegexOptions.IgnoreCase);
            var maxMatch = Regex.Match(key, @"^_max(.+)$", RegexOptions.IgnoreCase);

            if (minMatch.Success)
            {
                var field = FirstLower(minMatch.Groups[1].Value);
                if (rangeFilters.TryGetValue(field, out var existing))
                    rangeFilters[field] = (value, existing.Max);
                else
                    rangeFilters[field] = (value, null);
            }
            else if (maxMatch.Success)
            {
                var field = FirstLower(maxMatch.Groups[1].Value);
                if (rangeFilters.TryGetValue(field, out var existing))
                    rangeFilters[field] = (existing.Min, value);
                else
                    rangeFilters[field] = (null, value);
            }
            else
            {
                // Handle regular filters (equality and wildcards)
                if (value.Contains('*'))
                {
                    qp.AddWildcardFilter(key, value);
                }
                else
                {
                    qp.AddFilter(key, value);
                }
            }
        }

        // Add range filters to the main filters dictionary
        foreach (var (field, (min, max)) in rangeFilters)
        {
            qp.AddRangeFilter(field, min ?? string.Empty, max ?? string.Empty);
        }

        return qp;

        static int TryInt(string? v, int d) => int.TryParse(v, out var i) ? i : d;
        static string FirstLower(string s) => string.IsNullOrEmpty(s) ? s : char.ToLowerInvariant(s[0]) + s[1..];
    }

    public static IEnumerable<(string Field, int Direction)> ParseOrder(this string? order)
    {
        if (string.IsNullOrWhiteSpace(order)) yield break;

        var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var p in parts)
        {
            var seg = p.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var field = seg[0];
            var dir = seg.Length > 1 && seg[1].Equals("desc", StringComparison.OrdinalIgnoreCase) ? -1 : 1;
            yield return (field, dir);
        }
    }
}
