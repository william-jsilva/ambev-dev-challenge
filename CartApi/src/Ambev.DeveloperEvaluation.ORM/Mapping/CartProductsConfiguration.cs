using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class CartProductsConfiguration : IEntityTypeConfiguration<CartProduct>
{
    public void Configure(EntityTypeBuilder<CartProduct> builder)
    {
        builder.ToTable("CartProducts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
        builder.Property(c => c.CartId).IsRequired().HasColumnType("uuid");
        builder.Property(c => c.ProductId).IsRequired().HasColumnType("uuid");
        builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Quantity).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();
        builder.Property(c => c.UpdatedAt);
        builder.Property(c => c.DeletedAt);

        builder.HasOne(c => c.Cart).WithMany(c => c.Products).HasForeignKey(o => o.CartId);

        builder.HasIndex(c => new { c.CartId, c.ProductId, c.DeletedAt }).IsUnique();
        builder.HasIndex(c => c.Status);
    }
}
