namespace Ambev.DeveloperEvaluation.Application.Dtos;

public class ProductDto
{
    public string Id { get; set; } = default!;
    public string Title { get; set; } = default!;
    public double Price { get; set; }
    public string Description { get; set; } = default!;
    public string Category { get; set; } = default!;
    public string Image { get; set; } = default!;
    public RatingDto Rating { get; set; } = new();
}