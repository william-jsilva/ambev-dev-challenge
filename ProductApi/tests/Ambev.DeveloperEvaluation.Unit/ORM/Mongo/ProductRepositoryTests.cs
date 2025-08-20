using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Mongo;
using Bogus;
using FluentAssertions;
using MongoDB.Driver;
using NSubstitute;
using Xunit;

namespace Ambev.DeveloperEvaluation.Unit.ORM.Mongo;

public class ProductRepositoryTests
{
    private readonly MongoContext _context;
    private readonly ProductRepository _repository;
    private readonly Faker<Product> _productFaker;

    public ProductRepositoryTests()
    {
        _context = Substitute.For<MongoContext>();
        _repository = new ProductRepository(_context);
        
        _productFaker = new Faker<Product>()
            .RuleFor(p => p.Id, f => f.Random.Guid().ToString())
            .RuleFor(p => p.Title, f => f.Commerce.ProductName())
            .RuleFor(p => p.Price, f => f.Random.Double(1, 1000))
            .RuleFor(p => p.Description, f => f.Lorem.Sentence())
            .RuleFor(p => p.Category, f => f.Commerce.Categories(1)[0])
            .RuleFor(p => p.Image, f => f.Image.PicsumUrl())
            .RuleFor(p => p.Rating, f => new Rating
            {
                Rate = f.Random.Double(0, 5),
                Count = f.Random.Int(0, 1000)
            });
    }

    [Fact]
    public async Task GetAsync_ValidQuery_ShouldReturnProducts()
    {
        // Arrange
        var query = new QueryParameters { Page = 1, Size = 10 };
        var products = _productFaker.Generate(5);
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockFindFluent = Substitute.For<IFindFluent<Product, Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.Find(Arg.Any<FilterDefinition<Product>>()).Returns(mockFindFluent);
        mockCollection.CountDocumentsAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<CountOptions>(), Arg.Any<CancellationToken>())
            .Returns(5);
        mockFindFluent.Sort(Arg.Any<SortDefinition<Product>>()).Returns(mockFindFluent);
        mockFindFluent.Skip(Arg.Any<int>()).Returns(mockFindFluent);
        mockFindFluent.Limit(Arg.Any<int>()).Returns(mockFindFluent);
        mockFindFluent.ToListAsync(Arg.Any<CancellationToken>()).Returns(products);

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.Total.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var productId = "test-id-123";
        var product = _productFaker.Generate();
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockFindFluent = Substitute.For<IFindFluent<Product, Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.Find(Arg.Any<FilterDefinition<Product>>()).Returns(mockFindFluent);
        mockFindFluent.FirstOrDefaultAsync(Arg.Any<CancellationToken>()).Returns(product);

        // Act
        var result = await _repository.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var productId = "non-existing-id";
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockFindFluent = Substitute.For<IFindFluent<Product, Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.Find(Arg.Any<FilterDefinition<Product>>()).Returns(mockFindFluent);
        mockFindFluent.FirstOrDefaultAsync(Arg.Any<CancellationToken>()).Returns((Product?)null);

        // Act
        var result = await _repository.GetByIdAsync(productId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ValidProduct_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var product = _productFaker.Generate();
        product.Id = ""; // Empty ID to test auto-generation
        var mockCollection = Substitute.For<IMongoCollection<Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.InsertOneAsync(Arg.Any<Product>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.CreateAsync(product, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
        result.Id.Should().NotBeEmpty(); // Should have generated ID
        await mockCollection.Received(1).InsertOneAsync(Arg.Any<Product>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ProductWithExistingId_ShouldNotGenerateNewId()
    {
        // Arrange
        var product = _productFaker.Generate();
        var originalId = product.Id;
        var mockCollection = Substitute.For<IMongoCollection<Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.InsertOneAsync(Arg.Any<Product>(), Arg.Any<InsertOneOptions>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _repository.CreateAsync(product, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(originalId); // Should keep original ID
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_ShouldUpdateAndReturnProduct()
    {
        // Arrange
        var productId = "test-id-123";
        var product = _productFaker.Generate();
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockUpdateResult = Substitute.For<ReplaceOneResult>();

        _context.Products.Returns(mockCollection);
        mockUpdateResult.ModifiedCount.Returns(1);
        mockCollection.ReplaceOneAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<Product>(), Arg.Any<ReplaceOptions>(), Arg.Any<CancellationToken>())
            .Returns(mockUpdateResult);

        // Act
        var result = await _repository.UpdateAsync(productId, product, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product);
        result.Id.Should().Be(productId); // Should have the correct ID
        await mockCollection.Received(1).ReplaceOneAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<Product>(), Arg.Any<ReplaceOptions>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var productId = "non-existing-id";
        var product = _productFaker.Generate();
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockUpdateResult = Substitute.For<ReplaceOneResult>();

        _context.Products.Returns(mockCollection);
        mockUpdateResult.ModifiedCount.Returns(0);
        mockCollection.ReplaceOneAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<Product>(), Arg.Any<ReplaceOptions>(), Arg.Any<CancellationToken>())
            .Returns(mockUpdateResult);

        // Act
        var result = await _repository.UpdateAsync(productId, product, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ValidProductId_ShouldDeleteProduct()
    {
        // Arrange
        var productId = "test-id-123";
        var mockCollection = Substitute.For<IMongoCollection<Product>>();

        _context.Products.Returns(mockCollection);
        mockCollection.DeleteOneAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _repository.DeleteAsync(productId, CancellationToken.None);

        // Assert
        await mockCollection.Received(1).DeleteOneAsync(Arg.Any<FilterDefinition<Product>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnCategories()
    {
        // Arrange
        var categories = new[] { "Electronics", "Clothing", "Books" };
        var mockCollection = Substitute.For<IMongoCollection<Product>>();
        var mockDistinctFluent = Substitute.For<IAsyncCursor<string>>();

        _context.Products.Returns(mockCollection);
        mockDistinctFluent.Current.Returns(categories);
        mockDistinctFluent.MoveNextAsync(Arg.Any<CancellationToken>()).Returns(true, false);
        mockCollection.DistinctAsync<string>(Arg.Any<string>(), Arg.Any<FilterDefinition<Product>>(), Arg.Any<DistinctOptions>(), Arg.Any<CancellationToken>())
            .Returns(mockDistinctFluent);

        // Act
        var result = await _repository.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(categories);
    }
}
