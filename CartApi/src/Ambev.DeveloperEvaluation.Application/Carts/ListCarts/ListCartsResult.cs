using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

/// <summary>
/// Response model for ListCarts operation
/// </summary>
public class ListCartsResult
{
    /// <summary>
    /// List of carts returned by the operation
    /// </summary>
    public IEnumerable<Cart> Carts { get; set; } = [];

    /// <summary>
    /// Total number of items in the list
    /// </summary>
    public int TotalItems { get; set; }
}