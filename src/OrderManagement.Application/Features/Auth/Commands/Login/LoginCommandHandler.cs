using MediatR;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler(IAuthService authService)
    : IRequestHandler<LoginCommand, Result<TokenResponse>>
{
    public Task<Result<TokenResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        return authService.LoginAsync(
            request.Email.Trim(),
            request.Password,
            cancellationToken);
    }
}
