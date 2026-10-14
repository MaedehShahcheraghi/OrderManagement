using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Application.Features.Auth.Commands.Login;
using OrderManagement.Infrastructure.Identity.Constance;
using OrderManagement.Infrastructure.Identity.Options;
using OrderManagement.Infrastructure.Identity.UserModels;

namespace OrderManagement.Infrastructure.Identity.Services;

public sealed class JwtTokenService(
    IOptions<JwtOptions> options,
    TimeProvider timeProvider)
{
    public TokenResponse Create(ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(user.SecurityStamp))
        {
            throw new InvalidOperationException(
                "The account has no security stamp.");
        }

        JwtOptions settings = options.Value;

        DateTimeOffset now = timeProvider.GetUtcNow();
        DateTimeOffset expiresAt =
            now.AddMinutes(settings.AccessTokenMinutes);

        Claim[] claims =
        [
            new(JwtRegisteredClaimNames.Sub, user.Id),

            new(JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString("N")),

            new(
                JwtRegisteredClaimNames.Iat,
                now.ToUnixTimeSeconds()
                    .ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64),

            new(JwtClaimNames.SecurityStamp, user.SecurityStamp)
        ];

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.SigningKey));

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256));

        return new TokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer",
            expiresAt);
    }
}
