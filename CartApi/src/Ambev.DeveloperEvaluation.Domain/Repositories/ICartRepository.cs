using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Cart entity operations
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Creates a new cart in the repository
    /// </summary>
    /// <param name="cart">The cart to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created cart</returns>
    Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a cart from the repository
    /// </summary>
    /// <param name="cart">Cart informations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cart by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the cart</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart if found, null otherwise</returns>
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a cart active by userId
    /// </summary>
    /// <param name="userId">The userId to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart if found, null otherwise</returns>
    Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of carts with optional ordering
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>page with carts</returns>
    Task<IEnumerable<Cart>> GetAsync(int page, int size, string? order, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the total count of carts
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>total count of carts</returns>
    Task<int> GetTotalAsync(CancellationToken cancellationToken);
}
