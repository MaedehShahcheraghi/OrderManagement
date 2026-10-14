using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Authorization;
using OrderManagement.Api.Common;
using OrderManagement.Application.Common.Security;
using OrderManagement.Application.Features.Orders.Commands;
using OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;
using OrderManagement.Application.Features.Orders.Models;
using OrderManagement.Application.Features.Orders.Queries;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Endpoints;

public static class OrderEndpoints
{
    public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders").WithTags("Orders").RequireAuthorization();

        group.MapPost("/", async (CreateOrderCommand input, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(input, token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.CreateOrder);

        group.MapPost("/bulk", async (BulkCreateOrdersCommand input, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(input, token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.BulkCreateOrders);

        group.MapGet("/{id:long}", async (long id, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new GetOrderByIdQuery(id), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.ReadOrders);

        group.MapGet("/", async (
            [AsParameters] OrderSearch search, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new GetOrdersQuery(
                new OrderFilter(search.CustomerId, search.Status, search.From, search.To),
                search.PageNumber, search.PageSize), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.ReadOrders);

        group.MapGet("/after", async (
            [AsParameters] OrderCursorSearch search, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new GetOrdersAfterQuery(
                new OrderFilter(search.CustomerId, search.Status, search.From, search.To),
                search.AfterId, search.PageSize), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.ReadOrders);

        group.MapPost("/{id:long}/confirm", async (long id, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new ConfirmOrderCommand(id), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.ConfirmOrder);

        group.MapPost("/{id:long}/ship", async (long id, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new ShipOrderCommand(id), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.ShipOrder);

        group.MapPost("/{id:long}/deliver", async (long id, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new DeliverOrderCommand(id), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.DeliverOrder);

        group.MapDelete("/{id:long}", async (long id, ISender sender, HttpContext context, CancellationToken token) =>
            (await sender.Send(new DeleteOrderCommand(id), token)).ToHttpResult(context))
            .RequireAuthorization(PermissionPolicyProvider.Prefix + PermissionCodes.DeleteOrder);
    }

    public sealed record OrderSearch(
        long? CustomerId = null,
        OrderStatus? Status = null,
        DateTimeOffset? From = null,
        DateTimeOffset? To = null,
        int PageNumber = 1,
        int PageSize = 20);

    public sealed record OrderCursorSearch(
        long? CustomerId = null,
        OrderStatus? Status = null,
        DateTimeOffset? From = null,
        DateTimeOffset? To = null,
        long? AfterId = null,
        int PageSize = 20);
}
