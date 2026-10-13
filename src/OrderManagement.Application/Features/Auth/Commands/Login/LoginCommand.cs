using FluentValidation;
using MediatR;
using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Features.Auth.Commands.Login;


public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<Result<TokenResponse>>
{
    public override string ToString() => nameof(LoginCommand);
}

