namespace Ambev.DeveloperEvaluation.ORM.Mongo;

public class MongoOptions
{
    public string ConnectionString { get; set; } = default!;
    public string Database { get; set; } = default!;
    public string Collection { get; set; } = "products";
}
