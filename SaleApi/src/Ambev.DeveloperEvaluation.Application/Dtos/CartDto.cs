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