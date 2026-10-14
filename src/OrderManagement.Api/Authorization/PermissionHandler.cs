using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Infrastructure.Identity.Contexts;

namespace OrderManagement.Api.Authorization;

public sealed class PermissionHandler(
    IdentityDBContext db,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        string? userId = context.User
            .FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (context.User.Identity?.IsAuthenticated != true
            || string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        CancellationToken cancellationToken =
            httpContextAccessor.HttpContext?.RequestAborted
            ?? CancellationToken.None;

        bool allowed = await (
            from userRole in db.UserRoles
            join rolePermission in db.RolePermissions
                on userRole.RoleId equals rolePermission.RoleId
            join permission in db.Permissions
                on rolePermission.PermissionId equals permission.Id
            where userRole.UserId == userId
                  && permission.Code == requirement.Code
            select permission.Id
        ).AnyAsync(cancellationToken);

        if (allowed)
        {
            context.Succeed(requirement);
        }
    }
}
