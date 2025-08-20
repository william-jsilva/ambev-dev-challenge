using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken ct);
    Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken ct);
    Task<(IEnumerable<Product> Items, int Total)> GetAsync(QueryParameters query, CancellationToken ct);
    
    Task<Product> CreateAsync(Product product, CancellationToken ct);
    Task<Product?> GetByIdAsync(string id, CancellationToken ct);
    Task<Product?> UpdateAsync(string id, Product product, CancellationToken ct);
    Task DeleteAsync(string id, CancellationToken ct);
}
