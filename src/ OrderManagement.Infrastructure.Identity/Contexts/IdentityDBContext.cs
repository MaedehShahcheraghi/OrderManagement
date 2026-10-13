using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Infrastructure.Identity.Contexts;

public sealed class IdentityDBContext(
    DbContextOptions<IdentityDBContext> options)
    : Microsoft.AspNetCore.Identity.EntityFrameworkCore
        .IdentityDbContext<ApplicationUser>(options)
{
    public const string Schema = "identity";

    public DbSet<Permission> Permissions =>
        Set<Permission>();

    public DbSet<RolePermission> RolePermissions =>
        Set<RolePermission>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema(Schema);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdentityDBContext).Assembly);
    }
}
