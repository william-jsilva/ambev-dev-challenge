using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.Application.Sales.ListSales;

/// <summary>
/// Response model for ListSales operation
/// </summary>
public class ListSalesResult
{
    /// <summary>
    /// List of sales returned by the operation
    /// </summary>
    public IEnumerable<SaleDto> Sales { get; set; } = [];

    /// <summary>
    /// Total number of items in the list
    /// </summary>
    public int TotalItems { get; set; }
}