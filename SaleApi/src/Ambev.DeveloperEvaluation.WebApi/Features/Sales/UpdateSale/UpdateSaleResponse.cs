using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;

/// <summary>
/// API response model for UpdateSale operation
/// </summary>
/// <param name="Id">The unique identifier of the sale</param>
/// <param name="UserId">User Id</param>
/// <param name="Date">Date of sale</param>
/// <param name="Products">List of produts</param>
public record UpdateSaleResponse
(
    Guid Id,
    Guid UserId,
    DateTimeOffset Date,
    IEnumerable<SaleProduct> Products
);