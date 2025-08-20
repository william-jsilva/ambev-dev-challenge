using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using MongoDB.Driver;
using MongoDB.Bson;

namespace Ambev.DeveloperEvaluation.ORM.Mongo;

public class ProductRepository : IProductRepository
{
    private readonly MongoContext _ctx;

    public ProductRepository(MongoContext ctx) => _ctx = ctx;

    public async Task<(IEnumerable<Product> Items, int Total)> GetAsync(QueryParameters query, CancellationToken ct)
    {
        var (filter, sort) = BuildFilterAndSort(query);

        var total = (int)await _ctx.Products
            .CountDocumentsAsync(filter, cancellationToken: ct);

        var items = await _ctx.Products.Find(filter)
            .Sort(sort)
            .Skip((query.Page - 1) * query.Size)
            .Limit(query.Size)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync(CancellationToken ct)
    {
        return await _ctx.Products
            .Distinct<string>("Category", FilterDefinition<Product>.Empty)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(string category, CancellationToken ct)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
        return await _ctx.Products.Find(filter).ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(string id, CancellationToken ct)
    {
        return await _ctx.Products
            .Find(p => p.Id == id)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Product?> CreateAsync(Product product, CancellationToken ct)
    {
        // Generate ID if not provided
        if (string.IsNullOrEmpty(product.Id))
        {
            product.Id = ObjectId.GenerateNewId().ToString();
        }

        await _ctx.Products.InsertOneAsync(product, cancellationToken: ct);
        return product;
    }

    public async Task<Product?> UpdateAsync(string id, Product product, CancellationToken ct)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        
        // Ensure the product has the correct ID
        product.Id = id;
        
        var result = await _ctx.Products.ReplaceOneAsync(filter, product, cancellationToken: ct);
        
        // Return null if no document was updated
        return result.ModifiedCount > 0 ? product : null;
    }

    public async Task DeleteAsync(string id, CancellationToken ct)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
        await _ctx.Products.DeleteOneAsync(filter, ct);
    }

    private static (FilterDefinition<Product> Filter, SortDefinition<Product> Sort) BuildFilterAndSort(QueryParameters query)
    {
        var builder = Builders<Product>.Filter;
        var filter = builder.Empty;

        // Process filters: equality, wildcards, and ranges
        foreach (var param in query.Filters)
        {
            if (param.Value.Contains("*"))
            {
                var regex = param.Value.Replace("*", ".*");
                filter &= builder.Regex(param.Key, new MongoDB.Bson.BsonRegularExpression(regex, "i"));
            }
            else if (param.Key.EndsWith("_min"))
            {
                var field = param.Key.Replace("_min", "");
                if (double.TryParse(param.Value, out var minVal))
                    filter &= builder.Gte(field, minVal);
            }
            else if (param.Key.EndsWith("_max"))
            {
                var field = param.Key.Replace("_max", "");
                if (double.TryParse(param.Value, out var maxVal))
                    filter &= builder.Lte(field, maxVal);
            }
            else
            {
                filter &= builder.Eq(param.Key, param.Value);
            }
        }

        // Build sort definition
        var sortBuilder = Builders<Product>.Sort;
        SortDefinition<Product> sort = null;

        if (!string.IsNullOrWhiteSpace(query.Order))
        {
            var fields = query.Order.Split(',');
            foreach (var f in fields)
            {
                var isDesc = f.StartsWith("-");
                var fieldName = isDesc ? f.Substring(1) : f;
                sort = sort == null
                    ? (isDesc ? sortBuilder.Descending(fieldName) : sortBuilder.Ascending(fieldName))
                    : (isDesc ? sort.Descending(fieldName) : sort.Ascending(fieldName));
            }
        }

        return (filter, sort ?? sortBuilder.Ascending(nameof(Product.Id)));
    }
}
