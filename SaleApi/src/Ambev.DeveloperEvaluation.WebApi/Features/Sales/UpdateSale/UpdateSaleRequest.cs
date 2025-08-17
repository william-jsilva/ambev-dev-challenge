using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// Represents a request to update a sale in the system.
/// </summary>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of sale</param>
/// <param name="Products">List of produts</param>
public record UpdateSaleRequest
(
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<SaleProductDto> Products
);