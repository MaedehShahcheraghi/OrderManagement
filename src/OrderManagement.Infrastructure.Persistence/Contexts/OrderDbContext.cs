using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Customers;
using OrderManagement.Domain.Entities.Inventory;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Entities.Products;

namespace OrderManagement.Infrastructure.Persistence.Contexts;

public sealed class OrderDbContext(
    DbContextOptions<OrderDbContext> options)
    : DbContext(options)
{
    public const string Schema = "sales";

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderStatusHistory> OrderStatusHistories =>
        Set<OrderStatusHistory>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<InventoryStock> InventoryStocks =>
        Set<InventoryStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderDbContext).Assembly);
    }
}
