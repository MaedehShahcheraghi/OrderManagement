using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Repositories;
using OrderManagement.Infrastructure.Persistence.Contexts;
using OrderManagement.Infrastructure.Persistence.Repositories;
using OrderManagement.Infrastructure.Persistence.SeedData;

namespace OrderManagement.Infrastructure.Persistence.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<OrderDbContext>(options =>
            options
                .UseSqlServer(
                    connectionString,
                    sql => sql.UseCompatibilityLevel(160))
                .UseSeeding((context, _) =>
                    DatabaseSeeder.Seed(
                        (OrderDbContext)context))
                .UseAsyncSeeding((context, _, token) =>
                    DatabaseSeeder.SeedAsync(
                        (OrderDbContext)context,
                        token)));

        services.AddScoped<
            IOrderWriteRepository,
            OrderWriteRepository>();

        services.AddScoped<
            IOrderReadRepository,
            OrderReadRepository>();

        services.AddScoped<
            ICustomerRepository,
            CustomerRepository>();

        services.AddScoped<
            IProductRepository,
            ProductRepository>();

        services.AddScoped<
            IInventoryRepository,
            InventoryRepository>();

        services.AddScoped<
            IUnitOfWork,
            UnitOfWork>();

        return services;
    }
}
