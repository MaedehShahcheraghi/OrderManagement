using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using OrderManagement.Application.Abstractions;
using OrderManagement.Infrastructure.Identity.Constance;

namespace OrderManagement.Api.Security;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor)
    : ICurrentUser
{
    private ClaimsPrincipal? Principal =>
        accessor.HttpContext?.User;

    public string? UserId =>
        Principal?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated == true;

    public long? CustomerId
    {
        get
        {
            string? value = Principal?
                .FindFirst(JwtClaimNames.CustomerId)?.Value;

            return long.TryParse(
                value,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out long id)
                ? id
                : null;
        }
    }

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) == true;
}
