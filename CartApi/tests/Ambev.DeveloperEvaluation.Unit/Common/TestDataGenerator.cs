using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public static class TestDataGenerator
{
    private static readonly Faker _faker = new Faker("pt_BR");

    public static class CartData
    {
        public static Cart GenerateValidCart()
        {
            return new Faker<Cart>("pt_BR")
                .RuleFor(c => c.Id, f => Guid.NewGuid())
                .RuleFor(c => c.UserId, f => Guid.NewGuid())
                .RuleFor(c => c.Date, f => f.Date.RecentOffset())
                .RuleFor(c => c.Status, f => f.PickRandom<CartStatus>())
                .RuleFor(c => c.Products, f => new List<CartProduct>())
                .RuleFor(c => c.CreatedAt, f => f.Date.RecentOffset())
                .RuleFor(c => c.UpdatedAt, f => f.Random.Double() < 0.3 ? f.Date.RecentOffset() : null)
                .RuleFor(c => c.DeletedAt, f => f.Random.Double() < 0.1 ? f.Date.RecentOffset() : null)
                .Generate();
        }

        public static List<CartProduct> GenerateCartProducts(int count)
        {
            var faker = new Faker<CartProduct>("pt_BR")
                .RuleFor(cp => cp.Id, f => Guid.NewGuid())
                .RuleFor(cp => cp.CartId, f => Guid.NewGuid())
                .RuleFor(cp => cp.ProductId, f => Guid.NewGuid())
                .RuleFor(cp => cp.Quantity, f => f.Random.Int(1, 20))
                .RuleFor(cp => cp.Status, f => f.PickRandom<CartProductStatus>())
                .RuleFor(cp => cp.CreatedAt, f => f.Date.RecentOffset())
                .RuleFor(cp => cp.UpdatedAt, f => f.Random.Double() < 0.3 ? f.Date.RecentOffset() : null)
                .RuleFor(cp => cp.DeletedAt, f => f.Random.Double() < 0.1 ? f.Date.RecentOffset() : null);
            
            return faker.Generate(count);
        }

        public static Cart GenerateCartWithSpecificProducts(int productCount, int quantity)
        {
            var cart = GenerateValidCart();
            cart.Products = Enumerable.Range(0, productCount)
                .Select(_ => new CartProduct
                {
                    Id = Guid.NewGuid(),
                    CartId = cart.Id,
                    ProductId = Guid.NewGuid(),
                    Quantity = quantity,
                    Status = CartProductStatus.Active,
                    CreatedAt = DateTimeOffset.UtcNow
                })
                .ToList();

            return cart;
        }
    }

    public static class CommonData
    {
        public static string GenerateValidEmail()
        {
            return _faker.Internet.Email();
        }

        public static string GenerateValidPhone()
        {
            return _faker.Phone.PhoneNumber("(##) #####-####");
        }

        public static string GenerateValidPassword()
        {
            return _faker.Internet.Password(8, 20, prefix: "Senha@");
        }

        public static string GenerateValidUsername()
        {
            return _faker.Internet.UserName();
        }

        public static Guid GenerateGuid()
        {
            return _faker.Random.Guid();
        }

        public static DateTimeOffset GenerateRecentDate()
        {
            return _faker.Date.RecentOffset();
        }

        public static decimal GeneratePrice(decimal min = 1.0m, decimal max = 100.0m)
        {
            return _faker.Random.Decimal(min, max);
        }

        public static int GenerateQuantity(int min = 1, int max = 20)
        {
            return _faker.Random.Int(min, max);
        }
    }
}
