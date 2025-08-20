namespace Ambev.DeveloperEvaluation.Application.Dtos;

/// <summary>
/// Data Transfer Object for Sale
/// </summary>
public class SaleDto
{
    /// <summary>
    /// Sale Id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Sale number
    /// </summary>
    public long SaleNumber { get; set; }

    /// <summary>
    /// Total amount of the sale
    /// </summary>
    public decimal TotalSaleAmount { get; set; }

    /// <summary>
    /// Branch where the sale was made
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// User Id
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Date of sale
    /// </summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>
    /// List of <see cref="SaleProductDto "/>
    /// </summary>
    public List<SaleProductDto> Products { get; set; } = [];
}