namespace OrderManagement.Application.Abstractions;


public interface ICurrentUser
{
    string? UserId { get; }
}
