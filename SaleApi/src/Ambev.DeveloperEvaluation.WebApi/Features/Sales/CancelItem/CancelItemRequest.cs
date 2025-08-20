namespace Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelItem;

/// <summary>
/// Request for cancelling a specific item in a sale
/// </summary>
public class CancelItemRequest
{
    /// <summary>
    /// The ID of the sale containing the item to cancel
    /// </summary>
    public Guid SaleId { get; set; }

    /// <summary>
    /// The ID of the product to cancel
    /// </summary>
    public Guid ProductId { get; set; }
}
