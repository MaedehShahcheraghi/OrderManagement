using Microsoft.AspNetCore.Identity;

namespace OrderManagement.Infrastructure.Identity.UserModels;

public sealed class ApplicationUser : IdentityUser
{
    public long? CustomerId { get; set; }

    public bool IsActive { get; set; } = true;
}
