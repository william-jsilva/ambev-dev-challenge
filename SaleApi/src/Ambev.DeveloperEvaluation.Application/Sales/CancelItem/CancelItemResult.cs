namespace Ambev.DeveloperEvaluation.Application.Sales.CancelItem;

/// <summary>
/// Result of cancelling an item in a sale
/// </summary>
public class CancelItemResult
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The ID of the cancelled product
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// The ID of the sale containing the cancelled product
    /// </summary>
    public Guid SaleId { get; set; }
}
