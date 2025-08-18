using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleProductsConfiguration : IEntityTypeConfiguration<SaleProduct>
    {
        public void Configure(EntityTypeBuilder<SaleProduct> builder)
        {
            builder.ToTable("SaleProducts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(c => c.SaleId).IsRequired().HasColumnType("uuid");
            builder.Property(c => c.ProductId).IsRequired().HasColumnType("uuid");
            builder.Property(c => c.UnitPrice).IsRequired().HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(c => c.Discounts).IsRequired().HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(c => c.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(c => c.Quantity).IsRequired();
            builder.Property(c => c.CreatedAt).IsRequired();
            builder.Property(c => c.UpdatedAt);
            builder.Property(c => c.DeletedAt);

            builder.HasOne(c => c.Sale).WithMany(c => c.Products).HasForeignKey(o => o.SaleId);

            builder.HasIndex(c => new { c.SaleId, c.ProductId, c.DeletedAt }).IsUnique();
            builder.HasIndex(c => c.Status);
        }
    }
}
