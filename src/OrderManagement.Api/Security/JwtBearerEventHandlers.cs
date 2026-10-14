using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Identity.Constance;
using OrderManagement.Infrastructure.Identity.Contexts;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Api.Security;

public static class JwtBearerEventHandlers
{
    public static async Task ValidateAccountAsync(
        TokenValidatedContext context)
    {
        string? userId = context.Principal?
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        string? stamp = context.Principal?
            .FindFirst(JwtClaimNames.SecurityStamp)?.Value;

        if (string.IsNullOrWhiteSpace(userId)
            || string.IsNullOrWhiteSpace(stamp)
            || context.Principal?.Identity is not ClaimsIdentity identity)
        {
            context.Fail("Invalid account.");
            return;
        }

        IdentityDBContext db = context.HttpContext.RequestServices
            .GetRequiredService<IdentityDBContext>();

        CancellationToken token =
            context.HttpContext.RequestAborted;

        ApplicationUser? user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == userId, token);

        if (user is null
            || !user.IsActive
            || !string.Equals(
                user.SecurityStamp,
                stamp,
                StringComparison.Ordinal))
        {
            context.Fail("Invalid account.");
            return;
        }

        List<string> roles = await (
            from userRole in db.UserRoles
            join role in db.Roles
                on userRole.RoleId equals role.Id
            where userRole.UserId == userId && role.Name != null
            select role.Name!
        ).ToListAsync(token);

        // Replace account data with current database values.
        foreach (Claim claim in identity
            .FindAll(ClaimTypes.Role)
            .Concat(identity.FindAll(JwtClaimNames.CustomerId))
            .ToArray())
        {
            identity.RemoveClaim(claim);
        }

        identity.AddClaims(
            roles.Select(role =>
                new Claim(ClaimTypes.Role, role)));

        if (user.CustomerId is long customerId)
        {
            identity.AddClaim(new Claim(
                JwtClaimNames.CustomerId,
                customerId.ToString(CultureInfo.InvariantCulture)));
        }
    }

    public static Task WriteProblemAsync(
        HttpContext context,
        int status,
        string code,
        string detail)
    {
        string title = status switch
        {
            401 => "Unauthorized",
            403 => "Forbidden",
            429 => "TooManyRequests",
            _ => "Error"
        };

        return Results.Problem(
            statusCode: status,
            title: title,
            detail: detail,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = code,
                ["traceId"] = context.TraceIdentifier
            }).ExecuteAsync(context);
    }
}
