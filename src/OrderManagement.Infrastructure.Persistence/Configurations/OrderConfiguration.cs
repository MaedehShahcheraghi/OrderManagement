using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagement.Domain.Entities.Customers;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.Configurations;


public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders", schema: OrderDbContext.Schema, table =>
        {
            table.HasCheckConstraint(
                "CK_Orders_TotalAmount",
                "[TotalAmount] > 0");

            table.HasCheckConstraint(
                "CK_Orders_Status",
                "[Status] BETWEEN 0 AND 3");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .UseIdentityColumn();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.DeletedBy)
            .HasMaxLength(200);

        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(x => x.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Items)
            .HasField("_items")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(x => x.StatusHistory)
            .WithOne()
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.StatusHistory)
            .HasField("_statusHistory")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(x => new { x.CustomerId, x.Id })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.Status, x.Id })
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => new { x.CreatedAt, x.Id })
            .HasFilter("[IsDeleted] = 0");
    }
}
