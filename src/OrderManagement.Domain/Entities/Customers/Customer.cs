using System.Net.Mail;
using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities.Customers;

public sealed class Customer : Entity
{
    private Customer() { }

    public string FullName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    public static Customer Create(string fullName, string email)
    {
        string validatedName = DomainGuard.RequiredText(fullName, 200, nameof(FullName));
        string validatedEmail = DomainGuard.RequiredText(email, 254, nameof(Email));

        if (!MailAddress.TryCreate(validatedEmail, out var address)
            || !string.Equals(address.Address, validatedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new DomainException("customer.invalid_email", "Email must be a valid email address.");
        }

        return new Customer { FullName = validatedName, Email = validatedEmail };
    }
}
