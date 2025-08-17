using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// Implementation of ICartRepository using Entity Framework Core
/// </summary>
public class CartRepository(DefaultContext context) : BaseRepository, ICartRepository
{
    private static readonly Dictionary<string, Expression<Func<Cart, object>>> allowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        ["id"] = c => c.Id,
        ["userId"] = c => c.UserId,
        ["date"] = c => c.Date
    };

    /// <summary>
    /// Creates a new cart in the database
    /// </summary>
    /// <param name="cart">The cart to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created cart</returns>
    public async Task<Cart> CreateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        await context.Carts.AddAsync(cart, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return cart;
    }

    /// <summary>
    /// Retrieves a cart by their unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the cart</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart if found, null otherwise</returns>
    public async Task<Cart?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Carts
            .Include(cart => cart.Products.Where(p => p.Status != CartProductStatus.Deleted)) // TODO: Add a specification
            .AsNoTracking()
            .Where(cart => cart.Status != CartStatus.Deleted) // TODO: Add a specification
            .FirstOrDefaultAsync(cart => cart.Id == id, cancellationToken);
    }

    /// <summary>
    /// Retrieves a cart active by userId
    /// </summary>
    /// <param name="userId">The userId to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The cart if found, null otherwise</returns>
    public async Task<Cart?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(user
            => user.UserId == userId // TODO: Add a specification
            && user.Status == CartStatus.Active,
            cancellationToken);
    }


    /// <summary>
    /// Deletes a cart from the database logically
    /// </summary>
    /// <param name="id">The unique identifier of the cart to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the cart was deleted, false if not found</returns>
    public async Task UpdateAsync(Cart cart, CancellationToken cancellationToken = default)
    {
        context.Update(cart);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTotalAsync(CancellationToken cancellationToken)
    {
        return await context.Carts.CountAsync(cancellationToken);
    }

    public async Task<IEnumerable<Cart>> GetAsync(int page, int size, string? order, CancellationToken cancellationToken)
    {
        IQueryable<Cart> query = context.Carts
            .AsNoTracking()
            .Where(cart => cart.Status != CartStatus.Deleted) // TODO: Add a specification
            .Include(cart => cart.Products.Where(p => p.Status != CartProductStatus.Deleted)); // TODO: Add a specification

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
