namespace OrderManagement.Infrastructure.Identity.UserModels;

public sealed class Permission
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}
