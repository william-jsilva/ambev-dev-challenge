using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Represents a request to create a new sale in the system.
/// </summary>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of sale</param>
/// <param name="Products">List of produts</param>
public record CreateSaleRequest
(
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<SaleProductDto> Products
);