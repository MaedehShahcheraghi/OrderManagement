using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Common.Security;
using OrderManagement.Infrastructure.Identity.Contexts;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Infrastructure.Identity.DataSeeders;

public sealed class IdentitySeeder(
    IdentityDBContext db,
    RoleManager<IdentityRole> roleManager,
    UserManager<ApplicationUser> userManager)
{
    public async Task SeedAsync(
        string adminEmail,
        string adminPassword,
        string userEmail,
        string userPassword,
        long customerId,
        CancellationToken token)
    {
        if (customerId <= 0
            || string.Equals(
                adminEmail,
                userEmail,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Identity bootstrap configuration is invalid.");
        }

        await using var transaction =
            await db.Database.BeginTransactionAsync(token);

        List<Permission> existing =
            await db.Permissions.ToListAsync(token);

        var permissions = existing.ToDictionary(
            x => x.Code,
            StringComparer.OrdinalIgnoreCase);

        foreach (string code in PermissionCodes.All)
        {
            if (permissions.ContainsKey(code))
            {
                continue;
            }

            var permission = new Permission
            {
                Code = code,
                Description = code
            };

            permissions.Add(code, permission);
            db.Permissions.Add(permission);
        }

        await db.SaveChangesAsync(token);

        var defaults = new Dictionary<string, string[]>
        {
            [RoleNames.Admin] = PermissionCodes.All.ToArray(),

            [RoleNames.User] =
            [
                PermissionCodes.ReadOrders,
                PermissionCodes.CreateOrder
            ]
        };

        foreach (var definition in defaults)
        {
            IdentityRole? role =
                await roleManager.FindByNameAsync(definition.Key);

            if (role is not null)
            {
                continue;
            }

            role = new IdentityRole(definition.Key);

            EnsureSucceeded(
                await roleManager.CreateAsync(role));

            db.RolePermissions.AddRange(
                definition.Value.Select(code =>
                    new RolePermission
                    {
                        RoleId = role.Id,
                        PermissionId = permissions[code].Id
                    }));
        }

        await db.SaveChangesAsync(token);

        await CreateUserIfMissingAsync(
            adminEmail,
            adminPassword,
            RoleNames.Admin,
            customerId: null);

        await CreateUserIfMissingAsync(
            userEmail,
            userPassword,
            RoleNames.User,
            customerId);

        await transaction.CommitAsync(token);
    }

    private async Task CreateUserIfMissingAsync(
        string email,
        string password,
        string role,
        long? customerId)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            CustomerId = customerId,
            IsActive = true
        };

        EnsureSucceeded(
            await userManager.CreateAsync(user, password));

        EnsureSucceeded(
            await userManager.AddToRoleAsync(user, role));
    }

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                string.Join(
                    "; ",
                    result.Errors.Select(x => x.Description)));
        }
    }
}

