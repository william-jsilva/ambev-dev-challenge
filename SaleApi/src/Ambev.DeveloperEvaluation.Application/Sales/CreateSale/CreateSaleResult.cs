using Ambev.DeveloperEvaluation.Application.Dtos;

namespace Ambev.DeveloperEvaluation.Application.Sales.CreateSale;

/// <summary>
/// Represents the response returned after successfully creating a new sale.
/// </summary>
/// <remarks>
/// This response contains the unique identifier of the newly created sale,
/// which can be used for subsequent operations or reference.
/// </remarks>
public struct CreateSaleResult()
{
    /// <summary>
    /// Gets or sets the unique identifier of the newly created sale.
    /// </summary>
    /// <value>A identifier the created sale in the system.</value>
    public Guid Id { get; set; } = default;

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; } = default;

    /// <summary>
    /// Date of sale
    /// </summary>
    public DateTimeOffset Date { get; set; } = default;

    /// <summary>
    /// List of <see cref="SaleProductDto" />
    /// </summary>
    public IEnumerable<SaleProductDto> Products { get; set; } = [];
}