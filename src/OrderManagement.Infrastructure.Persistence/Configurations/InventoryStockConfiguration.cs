using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities.Inventory;
using OrderManagement.Domain.Entities.Products;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Configurations;

public sealed class InventoryStockConfiguration
    : IEntityTypeConfiguration<InventoryStock>
{
    public void Configure(EntityTypeBuilder<InventoryStock> builder)
    {
        builder.ToTable("InventoryStocks", schema: OrderDbContext.Schema, table =>
            table.HasCheckConstraint(
                "CK_InventoryStocks_Quantity",
                "[AvailableQuantity] >= 0"));

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasOne<Product>()
            .WithOne()
            .HasForeignKey<InventoryStock>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.ProductId)
            .IsUnique();
    }
}
