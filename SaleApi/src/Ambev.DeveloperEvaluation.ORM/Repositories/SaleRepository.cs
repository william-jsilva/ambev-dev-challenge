using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ISaleRepository using Entity Framework Core
/// </summary>
public class SaleRepository(DefaultContext context) : BaseRepository, ISaleRepository
{
    private static readonly Dictionary<string, Expression<Func<Sale, object>>> allowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = c => c.Id,
        ["userId"] = c => c.UserId,
        ["date"] = c => c.Date
    };

    /// <summary>
    /// Creates a new sale in the database
    /// </summary>
    /// <param name="sale">The sale to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale</returns>
    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await context.Sales.AddAsync(sale, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    /// <summary>
    /// Retrieves a sale by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Sales
            .Include(sale => sale.Products.Where(p => p.Status != SaleProductStatus.Deleted)) // TODO: Add a specification
            .AsNoTracking()
            .Where(sale => sale.Status != SaleStatus.Deleted) // TODO: Add a specification
            .FirstOrDefaultAsync(sale => sale.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a sale active by userId
    /// </summary>
    /// <param name="userId">The userId to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale if found, null otherwise</returns>
    public async Task<Sale?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Sales
            .AsNoTracking()
            .FirstOrDefaultAsync(user
            => user.UserId == userId // TODO: Add a specification
            && user.Status == SaleStatus.Active,
            cancellationToken);
    }

    /// <summary>
    /// Deletes a sale from the database logically
    /// </summary>
    /// <param name="id">The unique identifier of the sale to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the sale was deleted, false if not found</returns>
    public async Task UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        context.Update(sale);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTotalAsync(CancellationToken cancellationToken)
    {
        return await context.Sales.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Sale>> GetAsync(int page, int size, string? order, CancellationToken cancellationToken)
    {
        IQueryable<Sale> query = context.Sales
            .AsNoTracking()
            .Where(sale => sale.Status != SaleStatus.Deleted) // TODO: Add a specification
            .Include(sale => sale.Products.Where(p => p.Status != SaleProductStatus.Deleted)); // TODO: Add a specification

        if (!string.IsNullOrWhiteSpace(order))
        {
            query = ApplyOrder(order, query, allowedFields);
        }
        else
        {
            query = query.OrderBy(c => c.Id);
        }

        return await query
            .Skip(size * (page - 1))
            .Take(size)
            .ToListAsync(cancellationToken);
    }
}
