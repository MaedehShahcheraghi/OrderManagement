namespace OrderManagement.Application.Features.Auth.Commands.Login;

public sealed record TokenResponse(
    string AccessToken,
    string TokenType,
    DateTimeOffset ExpiresAt);
