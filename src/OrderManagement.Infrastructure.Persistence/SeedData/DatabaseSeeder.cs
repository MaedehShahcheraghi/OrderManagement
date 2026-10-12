using Microsoft.EntityFrameworkCore;
using OrderManagement.Domain.Entities.Customers;
using OrderManagement.Domain.Entities.Inventory;
using OrderManagement.Domain.Entities.Products;
using OrderManagement.Infrastructure.Persistence.Contexts;

namespace OrderManagement.Infrastructure.Persistence.SeedData;

internal static class DatabaseSeeder
{
    private static readonly string[] Emails =
    [
        .. Enumerable.Range(1, 50)
            .Select(i => $"customer{i:000}@example.com")
    ];

    private static readonly string[] Skus =
    [
        .. Enumerable.Range(1, 200)
            .Select(i => $"SKU-{i:0000}")
    ];

    internal static void Seed(OrderDbContext db)
    {
        db.Customers.AddRange(
            MissingCustomers(
                db.Customers
                    .Where(x => Emails.Contains(x.Email))
                    .Select(x => x.Email)
                    .ToList()));

        db.Products.AddRange(
            MissingProducts(
                db.Products
                    .Where(x => Skus.Contains(x.Sku))
                    .Select(x => x.Sku)
                    .ToList()));

        db.SaveChanges();

        List<long> productIds = db.Products
            .Where(x => Skus.Contains(x.Sku))
            .Select(x => x.Id)
            .ToList();

        List<long> stockedIds = db.InventoryStocks
            .Where(x => productIds.Contains(x.ProductId))
            .Select(x => x.ProductId)
            .ToList();

        db.InventoryStocks.AddRange(
            MissingStocks(productIds, stockedIds));

        db.SaveChanges();
    }

    internal static async Task SeedAsync(
        OrderDbContext db,
        CancellationToken token)
    {
        List<string> emails = await db.Customers
            .Where(x => Emails.Contains(x.Email))
            .Select(x => x.Email)
            .ToListAsync(token);

        List<string> skus = await db.Products
            .Where(x => Skus.Contains(x.Sku))
            .Select(x => x.Sku)
            .ToListAsync(token);

        db.Customers.AddRange(MissingCustomers(emails));

        db.Products.AddRange(MissingProducts(skus));

        await db.SaveChangesAsync(token);

        List<long> productIds = await db.Products
            .Where(x => Skus.Contains(x.Sku))
            .Select(x => x.Id)
            .ToListAsync(token);

        List<long> stockedIds = await db.InventoryStocks
            .Where(x => productIds.Contains(x.ProductId))
            .Select(x => x.ProductId)
            .ToListAsync(token);

        db.InventoryStocks.AddRange(
            MissingStocks(productIds, stockedIds));

        await db.SaveChangesAsync(token);
    }

    private static IEnumerable<Customer> MissingCustomers(
        IEnumerable<string> existing)
    {
        var emails = existing.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        return Enumerable.Range(1, 50)
            .Where(i => !emails.Contains(Emails[i - 1]))
            .Select(i => Customer.Create(
                $"Customer {i:000}",
                Emails[i - 1]));
    }

    private static IEnumerable<Product> MissingProducts(
        IEnumerable<string> existing)
    {
        var skus = existing.ToHashSet(
            StringComparer.OrdinalIgnoreCase);

        return Enumerable.Range(1, 200)
            .Where(i => !skus.Contains(Skus[i - 1]))
            .Select(i => Product.Create(
                Skus[i - 1],
                $"Product {i:000}",
                10m + i));
    }

    private static IEnumerable<InventoryStock> MissingStocks(
        IEnumerable<long> productIds,
        IEnumerable<long> stockedIds) =>
        productIds
            .Except(stockedIds)
            .Select(id => InventoryStock.Create(id, 100));
}
