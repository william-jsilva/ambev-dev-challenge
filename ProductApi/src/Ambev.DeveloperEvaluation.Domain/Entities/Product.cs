namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Product
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public double Price { get; set; }
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Image { get; set; } = default!;
    public Rating Rating { get; set; } = new();
}
