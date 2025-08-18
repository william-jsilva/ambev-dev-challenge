using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping
{
    public class SaleConfiguration : IEntityTypeConfiguration<Sale>
    {
        public void Configure(EntityTypeBuilder<Sale> builder)
        {
            builder.ToTable("Sales");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id).HasColumnType("uuid").HasDefaultValueSql("gen_random_uuid()");
            builder.Property(u => u.UserId).IsRequired().HasColumnType("uuid");
            builder.Property(u => u.SaleNumber).IsRequired();
            builder.Property(u => u.Branch).IsRequired().HasMaxLength(100);
            builder.Property(u => u.TotalSaleAmount).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            builder.Property(u => u.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            builder.Property(u => u.Date).IsRequired();
            builder.Property(u => u.CreatedAt).IsRequired();
            builder.Property(u => u.UpdatedAt);
            builder.Property(u => u.DeletedAt);

            builder.HasIndex(c => new { c.UserId, c.DeletedAt }).IsUnique();
            builder.HasIndex(c => c.Status);
        }
    }
}
