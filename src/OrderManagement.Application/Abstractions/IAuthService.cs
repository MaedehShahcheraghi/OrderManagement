using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Features.Auth.Commands.Login;

namespace OrderManagement.Application.Abstractions;


public interface IAuthService
{
    Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken);
}
