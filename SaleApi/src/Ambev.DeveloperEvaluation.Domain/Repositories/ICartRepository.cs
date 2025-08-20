using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Cart entity operations
/// </summary>
public interface ICartRepository
{
    Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
