namespace OrderManagement.Infrastructure.Identity.UserModels;

public sealed class RolePermission
{
    public string RoleId { get; set; } = string.Empty;

    public int PermissionId { get; set; }
}
