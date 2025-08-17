using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale entity operations
/// </summary>
public interface ISaleRepository
{
    /// <summary>
    /// Creates a new sale in the repository
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale</returns>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a sale from the repository
    /// </summary>
    /// <param name="sale">Sale informations</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a sale active by userId
    /// </summary>
    /// <param name="userId">The userId to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    Task<Sale?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a paginated list of sales with optional ordering
    /// </summary>
    /// <param name="page"></param>
    /// <param name="size"></param>
    /// <param name="order"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>page with sales</returns>
    Task<IEnumerable<Sale>> GetAsync(int page, int size, string? order, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the total count of sales
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>total count of sales</returns>
    Task<int> GetTotalAsync(CancellationToken cancellationToken);
}
