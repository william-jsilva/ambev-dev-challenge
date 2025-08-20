using Microsoft.AspNetCore.Mvc;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;

/// <summary>
/// Request model for getting a sale by ID
/// </summary>
public class ListSalesRequest
{
    /// <summary>
    /// Page number for pagination
    /// </summary>
    /// <remarks>
    /// Page number is optional (default 1)
    /// Page number must be greater than or equal to 1
    /// </remarks>
    [FromQuery(Name = "_page")]
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <remarks>
    /// Number of items is optional (default 10)
    /// Number of items must be between 1 and 100 
    /// </remarks>
    [FromQuery(Name = "_size")]
    public int Size { get; set; } = 10;

    /// <summary>
    /// Ordering of results (e.g., "id desc, userId asc")
    /// </summary>
    /// <remarks>
    /// Order must be in the format: 'field [asc|desc], field2 [asc|desc]'.
    /// Fields must be one of: id, userId and date.
    /// </remarks>
    [FromQuery(Name = "_order")]
    public string? Order { get; set; }
}
