using Microsoft.AspNetCore.Authorization;

namespace OrderManagement.Api.Authorization;

public sealed record PermissionRequirement(string Code)
    : IAuthorizationRequirement;
