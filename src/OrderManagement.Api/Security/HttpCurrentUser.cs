using OrderManagement.Application.Abstractions;

namespace OrderManagement.Api.Security;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
    public string? UserId
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;

            return user?.Identity?.IsAuthenticated == true
                ? user.FindFirst("sub")?.Value
                : null;
        }
    }

    public long? CustomerId { get; }
    public bool IsAuthenticated { get; }
    public bool IsInRole(string role)
    {
        throw new NotImplementedException();
    }
}
