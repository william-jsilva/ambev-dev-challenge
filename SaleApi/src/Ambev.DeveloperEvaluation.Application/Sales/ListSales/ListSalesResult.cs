using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Response model for ListSales operation
/// </summary>
public class ListSalesResult
{
    /// <summary>
    /// List of sales returned by the operation
    /// </summary>
    public IEnumerable<Sale> Sales { get; set; } = [];

    /// <summary>
    /// Total number of items in the list
    /// </summary>
    public int TotalItems { get; set; }
}