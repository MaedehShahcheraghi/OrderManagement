using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OrderManagement.Api.Authorization;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Security;
using OrderManagement.Infrastructure.Identity.Options;

namespace OrderManagement.Api.Security;

public static class DependencyInjection
{
    public const string AdminPolicy = "admin-only";

    public static IServiceCollection AddApiSecurity(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.Replace(
            ServiceDescriptor.Scoped<ICurrentUser, HttpCurrentUser>());

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(
                JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, settings) =>
            {
                JwtOptions jwt = settings.Value;

                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;

                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwt.Audience,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SigningKey)),

                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidAlgorithms =
                    [
                        SecurityAlgorithms.HmacSha256
                    ],

                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "sub",
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated =
                        JwtBearerEventHandlers.ValidateAccountAsync,

                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        context.Response.Headers.WWWAuthenticate =
                            "Bearer";

                        return JwtBearerEventHandlers.WriteProblemAsync(
                            context.HttpContext,
                            401,
                            "auth.unauthorized",
                            "A valid access token is required.");
                    },

                    OnForbidden = context =>
                        JwtBearerEventHandlers.WriteProblemAsync(
                            context.HttpContext,
                            403,
                            "auth.forbidden",
                            "Access denied.")
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;

            options.AddPolicy(
                AdminPolicy,
                policy => policy
                    .AddAuthenticationSchemes(
                        JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireRole(RoleNames.Admin));
        });

        services.Replace(
            ServiceDescriptor.Singleton<
                IAuthorizationPolicyProvider,
                PermissionPolicyProvider>());

        services.AddScoped<
            IAuthorizationHandler,
            PermissionHandler>();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = 429;

            options.AddPolicy(
                "login",
                context => RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.OnRejected = (context, _) =>
                new ValueTask(
                    JwtBearerEventHandlers.WriteProblemAsync(
                        context.HttpContext,
                        429,
                        "auth.rate_limited",
                        "Too many login attempts. Try again later."));
        });

        return services;
    }
}
