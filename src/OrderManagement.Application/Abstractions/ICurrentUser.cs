namespace OrderManagement.Application.Abstractions;


public interface ICurrentUser
{
    string? UserId { get; }

    long? CustomerId { get; }

    bool IsAuthenticated { get; }

    bool IsInRole(string role);
}
