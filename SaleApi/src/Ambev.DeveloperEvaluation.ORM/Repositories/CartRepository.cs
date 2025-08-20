using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using HttpClient
/// </summary>
public class CartRepository : ICartRepository
{
    /// <summary>
    /// Retrieves a cart by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // ToDO: usar HttpClientFactory

        return new Cart()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = DateTimeOffset.UtcNow,
            Products =
            [
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    UnitPrice = 10.0m
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    Quantity = 6,
                    UnitPrice = 20.0m
                }
            ]
        };
    }
}