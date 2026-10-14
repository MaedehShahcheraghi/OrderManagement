using Microsoft.AspNetCore.Identity;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Errors;
using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Features.Auth.Commands.Login;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Infrastructure.Identity.Services;

public sealed class AuthService(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    JwtTokenService tokenService) : IAuthService
{
    public async Task<Result<TokenResponse>> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ApplicationUser? user =
            await userManager.FindByEmailAsync(email);

        if (user is null || !user.IsActive || user.TwoFactorEnabled)
        {
            return Result<TokenResponse>.Failure(
                AuthErrors.InvalidCredentials);
        }

        SignInResult signInResult =
            await signInManager.CheckPasswordSignInAsync(
                user,
                password,
                lockoutOnFailure: true);

        cancellationToken.ThrowIfCancellationRequested();

        if (!signInResult.Succeeded)
        {
            return Result<TokenResponse>.Failure(
                AuthErrors.InvalidCredentials);
        }

        return Result<TokenResponse>.Success(
            tokenService.Create(user));
    }
}
