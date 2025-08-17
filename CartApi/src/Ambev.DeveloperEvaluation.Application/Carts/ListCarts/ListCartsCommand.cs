using MediatR;

namespace Ambev.DeveloperEvaluation.Application.Carts.ListCarts;

/// <summary>
/// Command for retrieving a list of carts with pagination and ordering
/// </summary>
public record ListCartsCommand : IRequest<ListCartsResult>
{
    /// <summary>
    /// Page number for pagination
    /// </summary>
    /// <remarks>
    /// Page number is optional (default 1)
    /// Page number must be greater than or equal to 1
    /// </remarks>
    public int Page { get; set; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    /// <remarks>
    /// Number of items is optional (default 10)
    /// Number of items must be between 1 and 100 
    /// </remarks>
    public int Size { get; set; }

    /// <summary>
    /// Ordering of results (e.g., "id desc, userId asc")
    /// </summary>
    /// <remarks>
    /// Order must be in the format: 'field [asc|desc], field2 [asc|desc]'.
    /// Fields must be one of: id, userId and date
    /// </remarks>
    public string? Order { get; set; }
}
