using Ambev.DeveloperEvaluation.Domain.Entities;
using MongoDB.Driver;

namespace Ambev.DeveloperEvaluation.ORM.Mongo;

public class MongoContext
{
    private readonly IMongoDatabase _db;
    public IMongoCollection<Product> Products { get; }

    public MongoContext(MongoOptions options)
    {
        var client = new MongoClient(options.ConnectionString);
        _db = client.GetDatabase(options.Database);
        Products = _db.GetCollection<Product>(options.Collection);
    }
}
