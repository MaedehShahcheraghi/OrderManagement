using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OrderManagement.Application.Abstractions;
using OrderManagement.Infrastructure.Identity.Contexts;
using OrderManagement.Infrastructure.Identity.DataSeeders;
using OrderManagement.Infrastructure.Identity.Options;
using OrderManagement.Infrastructure.Identity.Services;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Infrastructure.Identity.Extensions;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IdentityDBContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("SqlServer"),
                sql =>
                {
                    sql.UseCompatibilityLevel(160);

                    sql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        IdentityDBContext.Schema);
                }));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(5);
            })
            .AddRoles<IdentityRole>().AddSignInManager()
            .AddEntityFrameworkStores<IdentityDBContext>();
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.Issuer)
                    && !string.IsNullOrWhiteSpace(options.Audience)
                    && !string.IsNullOrWhiteSpace(options.SigningKey)
                    && Encoding.UTF8.GetByteCount(options.SigningKey) >= 32
                    && options.AccessTokenMinutes is >= 1 and <= 60,
                "JWT configuration is invalid.")
            .ValidateOnStart();

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IdentitySeeder>();

        return services;
    }
}
