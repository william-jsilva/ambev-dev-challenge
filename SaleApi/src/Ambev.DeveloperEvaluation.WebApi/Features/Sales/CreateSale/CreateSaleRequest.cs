namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;

/// <summary>
/// Represents a request to create a new sale in the system.
/// </summary>
/// <param name="Date">Date of sale</param>
/// <param name="Branch">Branch where the sale was made</param>
/// <param name="CartId">Cart Id associated with the sale</param>
public record CreateSaleRequest
(
    DateTimeOffset Date,
    string Branch,
    Guid CartId
);