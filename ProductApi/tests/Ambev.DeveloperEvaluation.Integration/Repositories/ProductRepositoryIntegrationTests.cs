using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.ORM.Mongo;
using Bogus;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Ambev.DeveloperEvaluation.Integration.Repositories;

public class ProductRepositoryIntegrationTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;
    private readonly ProductRepository _repository;
    private readonly Faker<Product> _productFaker;

    public ProductRepositoryIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
        _repository = fixture.ServiceProvider.GetRequiredService<ProductRepository>();
        
        _productFaker = new Faker<Product>()
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
    public async Task CreateAsync_ValidProduct_ShouldCreateAndReturnProduct()
    {
        // Arrange
        var product = _productFaker.Generate();
        product.Id = ""; // Empty ID to test auto-generation

        // Act
        var result = await _repository.CreateAsync(product, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(product, options => options.Excluding(p => p.Id));
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingProduct_ShouldReturnProduct()
    {
        // Arrange
        var product = _productFaker.Generate();
        var createdProduct = await _repository.CreateAsync(product, CancellationToken.None);

        // Act
        var result = await _repository.GetByIdAsync(createdProduct.Id, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(createdProduct);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = "non-existing-id";

        // Act
        var result = await _repository.GetByIdAsync(nonExistingId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_ShouldUpdateAndReturnProduct()
    {
        // Arrange
        var product = _productFaker.Generate();
        var createdProduct = await _repository.CreateAsync(product, CancellationToken.None);
        
        var updatedProduct = _productFaker.Generate();
        updatedProduct.Id = createdProduct.Id;

        // Act
        var result = await _repository.UpdateAsync(createdProduct.Id, updatedProduct, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(updatedProduct);
        result.Id.Should().Be(createdProduct.Id);
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ShouldReturnNull()
    {
        // Arrange
        var nonExistingId = "non-existing-id";
        var product = _productFaker.Generate();

        // Act
        var result = await _repository.UpdateAsync(nonExistingId, product, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_ShouldDeleteProduct()
    {
        // Arrange
        var product = _productFaker.Generate();
        var createdProduct = await _repository.CreateAsync(product, CancellationToken.None);

        // Act
        await _repository.DeleteAsync(createdProduct.Id, CancellationToken.None);

        // Assert
        var deletedProduct = await _repository.GetByIdAsync(createdProduct.Id, CancellationToken.None);
        deletedProduct.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithPagination_ShouldReturnCorrectResults()
    {
        // Arrange
        var products = _productFaker.Generate(15);
        foreach (var product in products)
        {
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var query = new QueryParameters { Page = 1, Size = 10 };

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(10);
        result.Total.Should().BeGreaterThanOrEqualTo(15);
    }

    [Fact]
    public async Task GetAsync_WithCategoryFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var electronicsProducts = _productFaker.Generate(5);
        foreach (var product in electronicsProducts)
        {
            product.Category = "Electronics";
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var clothingProducts = _productFaker.Generate(3);
        foreach (var product in clothingProducts)
        {
            product.Category = "Clothing";
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var query = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Filters = new Dictionary<string, string>
            {
                { "Category", "Electronics" }
            }
        };

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(5);
        result.Items.Should().OnlyContain(p => p.Category == "Electronics");
    }

    [Fact]
    public async Task GetAsync_WithPriceRangeFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var cheapProducts = _productFaker.Generate(3);
        foreach (var product in cheapProducts)
        {
            product.Price = 50.0;
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var expensiveProducts = _productFaker.Generate(3);
        foreach (var product in expensiveProducts)
        {
            product.Price = 500.0;
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var query = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Filters = new Dictionary<string, string>
            {
                { "Price_min", "100" },
                { "Price_max", "600" }
            }
        };

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(p => p.Price >= 100 && p.Price <= 600);
    }

    [Fact]
    public async Task GetAsync_WithWildcardFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var iphoneProducts = _productFaker.Generate(3);
        foreach (var product in iphoneProducts)
        {
            product.Title = "iPhone 15 Pro";
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var samsungProducts = _productFaker.Generate(2);
        foreach (var product in samsungProducts)
        {
            product.Title = "Samsung Galaxy S24";
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var query = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Filters = new Dictionary<string, string>
            {
                { "Title", "iPhone*" }
            }
        };

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().OnlyContain(p => p.Title.StartsWith("iPhone"));
    }

    [Fact]
    public async Task GetAsync_WithOrdering_ShouldReturnOrderedResults()
    {
        // Arrange
        var products = _productFaker.Generate(5);
        foreach (var product in products)
        {
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        var query = new QueryParameters 
        { 
            Page = 1, 
            Size = 10,
            Order = "Price"
        };

        // Act
        var result = await _repository.GetAsync(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeInAscendingOrder(p => p.Price);
    }

    [Fact]
    public async Task GetCategoriesAsync_ShouldReturnAllCategories()
    {
        // Arrange
        var categories = new[] { "Electronics", "Clothing", "Books", "Home & Garden" };
        foreach (var category in categories)
        {
            var product = _productFaker.Generate();
            product.Category = category;
            await _repository.CreateAsync(product, CancellationToken.None);
        }

        // Act
        var result = await _repository.GetCategoriesAsync(CancellationToken.None);

        // Assert
        result.Should().Contain(categories);
    }
}
