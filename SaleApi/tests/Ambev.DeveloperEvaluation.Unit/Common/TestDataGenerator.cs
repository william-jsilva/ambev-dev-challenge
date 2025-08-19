using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Enums;
using Bogus;

namespace Ambev.DeveloperEvaluation.Unit.Common;

public static class TestDataGenerator
{
    public static class SaleData
    {
        public static Sale GenerateSale()
        {
            return new Faker<Sale>("pt_BR")
                .RuleFor(s => s.Id, f => f.Random.Guid())
                .RuleFor(s => s.SaleNumber, f => f.Random.Int(1000, 9999))
                .RuleFor(s => s.UserId, f => f.Random.Guid())
                .RuleFor(s => s.Date, f => f.Date.PastOffset())
                .RuleFor(s => s.Branch, f => f.PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste"))
                .RuleFor(s => s.Status, f => f.PickRandom<SaleStatus>())
                .RuleFor(s => s.Products, f => GenerateSaleProducts(f.Random.Int(1, 5)))
                .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(s => s.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(s => s.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }

        public static List<Sale> GenerateSales(int count)
        {
            return new Faker<Sale>("pt_BR")
                .RuleFor(s => s.Id, f => f.Random.Guid())
                .RuleFor(s => s.SaleNumber, f => f.Random.Int(1000, 9999))
                .RuleFor(s => s.UserId, f => f.Random.Guid())
                .RuleFor(s => s.Date, f => f.Date.PastOffset())
                .RuleFor(s => s.Branch, f => f.PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste"))
                .RuleFor(s => s.Status, f => f.PickRandom<SaleStatus>())
                .RuleFor(s => s.Products, f => GenerateSaleProducts(f.Random.Int(1, 5)))
                .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(s => s.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(s => s.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate(count);
        }

        public static Sale GenerateSaleWithSpecificProducts(int productCount)
        {
            return new Faker<Sale>("pt_BR")
                .RuleFor(s => s.Id, f => f.Random.Guid())
                .RuleFor(s => s.SaleNumber, f => f.Random.Int(1000, 9999))
                .RuleFor(s => s.UserId, f => f.Random.Guid())
                .RuleFor(s => s.Date, f => f.Date.PastOffset())
                .RuleFor(s => s.Branch, f => f.PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste"))
                .RuleFor(s => s.Status, f => f.PickRandom<SaleStatus>())
                .RuleFor(s => s.Products, f => GenerateSaleProducts(productCount))
                .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(s => s.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(s => s.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }

        public static Sale GenerateSaleWithStatus(SaleStatus status)
        {
            return new Faker<Sale>("pt_BR")
                .RuleFor(s => s.Id, f => f.Random.Guid())
                .RuleFor(s => s.SaleNumber, f => f.Random.Int(1000, 9999))
                .RuleFor(s => s.UserId, f => f.Random.Guid())
                .RuleFor(s => s.Date, f => f.Date.PastOffset())
                .RuleFor(s => s.Branch, f => f.PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste"))
                .RuleFor(s => s.Status, status)
                .RuleFor(s => s.Products, f => GenerateSaleProducts(f.Random.Int(1, 5)))
                .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(s => s.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(s => s.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }

        public static Sale GenerateSaleForUser(Guid userId)
        {
            return new Faker<Sale>("pt_BR")
                .RuleFor(s => s.Id, f => f.Random.Guid())
                .RuleFor(s => s.SaleNumber, f => f.Random.Int(1000, 9999))
                .RuleFor(s => s.UserId, userId)
                .RuleFor(s => s.Date, f => f.Date.PastOffset())
                .RuleFor(s => s.Branch, f => f.PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste"))
                .RuleFor(s => s.Status, f => f.PickRandom<SaleStatus>())
                .RuleFor(s => s.Products, f => GenerateSaleProducts(f.Random.Int(1, 5)))
                .RuleFor(s => s.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(s => s.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(s => s.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }
    }

    public static class SaleProductData
    {
        public static SaleProduct GenerateSaleProduct()
        {
            return new Faker<SaleProduct>("pt_BR")
                .RuleFor(sp => sp.Id, f => f.Random.Guid())
                .RuleFor(sp => sp.SaleId, f => f.Random.Guid())
                .RuleFor(sp => sp.ProductId, f => f.Random.Guid())
                .RuleFor(sp => sp.Quantity, f => f.Random.Int(1, 50))
                .RuleFor(sp => sp.UnitPrice, f => f.Random.Decimal(1.0m, 100.0m))
                .RuleFor(sp => sp.Discounts, f => f.PickRandom(0.8m, 0.9m, 1.0m))
                .RuleFor(sp => sp.TotalAmount, (f, sp) => sp.Quantity * sp.UnitPrice * sp.Discounts)
                .RuleFor(sp => sp.Status, f => f.PickRandom<SaleProductStatus>())
                .RuleFor(sp => sp.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(sp => sp.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(sp => sp.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }

        public static List<SaleProduct> GenerateSaleProducts(int count)
        {
            return new Faker<SaleProduct>("pt_BR")
                .RuleFor(sp => sp.Id, f => f.Random.Guid())
                .RuleFor(sp => sp.SaleId, f => f.Random.Guid())
                .RuleFor(sp => sp.ProductId, f => f.Random.Guid())
                .RuleFor(sp => sp.Quantity, f => f.Random.Int(1, 50))
                .RuleFor(sp => sp.UnitPrice, f => f.Random.Decimal(1.0m, 100.0m))
                .RuleFor(sp => sp.Discounts, f => f.PickRandom(0.8m, 0.9m, 1.0m))
                .RuleFor(sp => sp.TotalAmount, (f, sp) => sp.Quantity * sp.UnitPrice * sp.Discounts)
                .RuleFor(sp => sp.Status, f => f.PickRandom<SaleProductStatus>())
                .RuleFor(sp => sp.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(sp => sp.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(sp => sp.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate(count);
        }

        public static SaleProduct GenerateSaleProductWithQuantity(int quantity)
        {
            return new Faker<SaleProduct>("pt_BR")
                .RuleFor(sp => sp.Id, f => f.Random.Guid())
                .RuleFor(sp => sp.SaleId, f => f.Random.Guid())
                .RuleFor(sp => sp.ProductId, f => f.Random.Guid())
                .RuleFor(sp => sp.Quantity, quantity)
                .RuleFor(sp => sp.UnitPrice, f => f.Random.Decimal(1.0m, 100.0m))
                .RuleFor(sp => sp.Discounts, f => f.PickRandom(0.8m, 0.9m, 1.0m))
                .RuleFor(sp => sp.TotalAmount, (f, sp) => sp.Quantity * sp.UnitPrice * sp.Discounts)
                .RuleFor(sp => sp.Status, f => f.PickRandom<SaleProductStatus>())
                .RuleFor(sp => sp.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(sp => sp.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(sp => sp.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }
    }

    public static class CartData
    {
        public static Cart GenerateCart()
        {
            return new Faker<Cart>("pt_BR")
                .RuleFor(c => c.Id, f => f.Random.Guid())
                .RuleFor(c => c.UserId, f => f.Random.Guid())
                .RuleFor(c => c.Date, f => f.Date.PastOffset())
                .RuleFor(c => c.Status, f => f.PickRandom<CartStatus>())
                .RuleFor(c => c.Products, f => GenerateCartProducts(f.Random.Int(1, 5)))
                .RuleFor(c => c.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(c => c.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(c => c.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate();
        }

        public static List<CartProduct> GenerateCartProducts(int count)
        {
            return new Faker<CartProduct>("pt_BR")
                .RuleFor(cp => cp.Id, f => f.Random.Guid())
                .RuleFor(cp => cp.CartId, f => f.Random.Guid())
                .RuleFor(cp => cp.ProductId, f => f.Random.Guid())
                .RuleFor(cp => cp.Quantity, f => f.Random.Int(1, 50))
                .RuleFor(cp => cp.UnitPrice, f => f.Random.Decimal(1.0m, 100.0m))
                .RuleFor(cp => cp.CreatedAt, f => f.Date.PastOffset())
                .RuleFor(cp => cp.UpdatedAt, f => f.Date.PastOffset().OrNull(f, 0.3f))
                .RuleFor(cp => cp.DeletedAt, f => f.Date.PastOffset().OrNull(f, 0.1f))
                .Generate(count);
        }
    }

    public static class CommonData
    {
        public static string GenerateValidBranch()
        {
            return new Faker("pt_BR").PickRandom("Loja Centro", "Loja Norte", "Loja Sul", "Loja Leste", "Loja Oeste");
        }

        public static DateTimeOffset GenerateValidDate()
        {
            return new Faker("pt_BR").Date.PastOffset();
        }

        public static int GenerateValidSaleNumber()
        {
            return new Faker("pt_BR").Random.Int(1000, 9999);
        }

        public static decimal GenerateValidPrice()
        {
            return new Faker("pt_BR").Random.Decimal(1.0m, 1000.0m);
        }

        public static int GenerateValidQuantity()
        {
            return new Faker("pt_BR").Random.Int(1, 100);
        }
    }
}
