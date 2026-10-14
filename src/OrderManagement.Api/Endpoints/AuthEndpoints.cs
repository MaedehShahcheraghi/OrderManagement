using System.Security.Claims;
using MediatR;
using OrderManagement.Api.Common;
using OrderManagement.Api.Common.Extensions;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Features.Auth.Commands.Login;

namespace OrderManagement.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints
            .MapGroup("/api/auth")
            .WithTags("Authentication");

        group.MapPost(
                "/login",
                async (
                    LoginCommand command,
                    ISender sender,
                    HttpContext context,
                    CancellationToken token) =>
                {
                    var result = await sender.Send(command, token);

                    return result.ToHttpResult(context);
                })
            .AllowAnonymous()
            .RequireRateLimiting("login")
            .Produces<TokenResponse>()
            .ProducesProblem(400)
            .ProducesProblem(401)
            .ProducesProblem(429);

        group.MapGet(
                "/me",
                (ICurrentUser currentUser, HttpContext context) =>
                    Results.Ok(new
                    {
                        currentUser.UserId,
                        currentUser.CustomerId,

                        Roles = context.User
                            .FindAll(ClaimTypes.Role)
                            .Select(x => x.Value)
                            .ToArray()
                    }))
            .RequireAuthorization();
    }
}
