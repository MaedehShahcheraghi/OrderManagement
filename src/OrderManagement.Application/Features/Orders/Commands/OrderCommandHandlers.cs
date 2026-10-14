using MediatR;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Errors;
using OrderManagement.Application.Common.Results;
using OrderManagement.Application.Common.Security;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Features.Orders.Commands;

internal static class OrderCreation
{
    public static async Task<Result<IReadOnlyList<Order>>> PrepareAsync(
        IReadOnlyList<NewOrder> requests, ICustomerRepository customers,
        IProductRepository products, TimeProvider clock, string actor,
        CancellationToken token)
    {
        if (requests.Count is < 1 or > 500)
            return Result<IReadOnlyList<Order>>.Failure(new("orders.invalid_batch", "Batch size must be 1 to 500.", ErrorType.Validation));
        if (requests.Any(x => x is null || x.CustomerId <= 0 || x.Items is null ||
            x.Items.Count is < 1 or > Order.MaximumItems ||
            x.Items.Any(i => i is null || i.ProductId <= 0 || i.Quantity <= 0)))
            return Result<IReadOnlyList<Order>>.Failure(new("orders.invalid_input", "Invalid order or order item.", ErrorType.Validation));

        var customerIds = requests.Select(x => x.CustomerId).Distinct().ToArray();
        var existing = await customers.GetExistingIdsAsync(customerIds, token);
        if (existing.Count != customerIds.Length)
            return Result<IReadOnlyList<Order>>.Failure(new("orders.unknown_customer", "Customer does not exist.", ErrorType.Validation));

        var productIds = requests.SelectMany(x => x.Items).Select(x => x.ProductId).Distinct().ToArray();
        var prices = (await products.GetByIdsAsync(productIds, token)).ToDictionary(x => x.Id);
        if (prices.Count != productIds.Length)
            return Result<IReadOnlyList<Order>>.Failure(new("orders.unknown_product", "Product does not exist.", ErrorType.Validation));

        var result = new List<Order>(requests.Count);
        try
        {
            foreach (var request in requests)
            {
                var itemData = request.Items
                    .GroupBy(x => x.ProductId)
                    .Select(g => new OrderItemData(g.Key, checked(g.Sum(x => (long)x.Quantity) is long total && total <= int.MaxValue ? (int)total : throw new OverflowException()), prices[g.Key].UnitPrice))
                    .ToArray();
                result.Add(Order.Create(request.CustomerId, itemData, clock.GetUtcNow(), actor));
            }
        }
        catch (Exception ex) when (ex is DomainException or OverflowException)
        {
            return Result<IReadOnlyList<Order>>.Failure(
                ex is DomainException d ? DomainErrorMapper.Map(d) : new("orders.invalid_quantity", "Quantity is too large.", ErrorType.Validation));
        }

        return Result<IReadOnlyList<Order>>.Success(result);
    }
}

public sealed class CreateOrderHandler(
    ICustomerRepository customers, IProductRepository products,
    IOrderWriteRepository orders, IUnitOfWork work,
    ICurrentUser user, TimeProvider clock)
    : IRequestHandler<CreateOrderCommand, Result<long>>
{
    public async Task<Result<long>> Handle(CreateOrderCommand request, CancellationToken token)
    {
        if (!user.IsAuthenticated) return Result<long>.Failure(CommonErrors.Unauthenticated);
        if (!user.IsInRole(RoleNames.Admin) && user.CustomerId != request.CustomerId)
            return Result<long>.Failure(CommonErrors.Forbidden);
        var created = await OrderCreation.PrepareAsync(
            [new NewOrder(request.CustomerId, request.Items)], customers, products, clock, user.UserId!, token);
        if (created.IsFailure) return Result<long>.Failure(created.Error!);
        var order = created.Value[0];
        orders.Add(order);
        await work.SaveChangesAsync(token);
        return Result<long>.Success(order.Id);
    }
}

public sealed class BulkCreateOrdersHandler(
    ICustomerRepository customers, IProductRepository products,
    IBulkOrderWriteRepository orders, IUnitOfWork work,
    ICurrentUser user, TimeProvider clock)
    : IRequestHandler<BulkCreateOrdersCommand, Result<IReadOnlyList<long>>>
{
    public async Task<Result<IReadOnlyList<long>>> Handle(BulkCreateOrdersCommand request, CancellationToken token)
    {
        if (!user.IsAuthenticated) return Result<IReadOnlyList<long>>.Failure(CommonErrors.Unauthenticated);
        if (!user.IsInRole(RoleNames.Admin)) return Result<IReadOnlyList<long>>.Failure(CommonErrors.Forbidden);
        if (request.Orders is null)
            return Result<IReadOnlyList<long>>.Failure(new("orders.invalid_batch", "Orders are required.", ErrorType.Validation));
        var created = await OrderCreation.PrepareAsync(request.Orders, customers, products, clock, user.UserId!, token);
        if (created.IsFailure) return Result<IReadOnlyList<long>>.Failure(created.Error!);
        await using var tx = await work.BeginTransactionAsync(token);
        orders.AddRange(created.Value);
        await work.SaveChangesAsync(token);
        await tx.CommitAsync(token);
        return Result<IReadOnlyList<long>>.Success(created.Value.Select(x => x.Id).ToArray());
    }
}

public sealed class ShipOrderHandler(IOrderWriteRepository orders, IUnitOfWork work,
    ICurrentUser user, TimeProvider clock) : IRequestHandler<ShipOrderCommand, Result>
{
    public Task<Result> Handle(ShipOrderCommand request, CancellationToken token) =>
        OrderTransitions.Execute(request.OrderId, orders, work, user, clock, token, (x, now, actor) => x.Ship(now, actor));
}
public sealed class DeliverOrderHandler(IOrderWriteRepository orders, IUnitOfWork work,
    ICurrentUser user, TimeProvider clock) : IRequestHandler<DeliverOrderCommand, Result>
{
    public Task<Result> Handle(DeliverOrderCommand request, CancellationToken token) =>
        OrderTransitions.Execute(request.OrderId, orders, work, user, clock, token, (x, now, actor) => x.Deliver(now, actor));
}
public sealed class DeleteOrderHandler(IOrderWriteRepository orders, IUnitOfWork work,
    ICurrentUser user, TimeProvider clock) : IRequestHandler<DeleteOrderCommand, Result>
{
    public Task<Result> Handle(DeleteOrderCommand request, CancellationToken token) =>
        OrderTransitions.Execute(request.OrderId, orders, work, user, clock, token, (x, now, actor) => x.Delete(now, actor));
}

internal static class OrderTransitions
{
    public static async Task<Result> Execute(long id, IOrderWriteRepository orders, IUnitOfWork work,
        ICurrentUser user, TimeProvider clock, CancellationToken token, Action<Order, DateTimeOffset, string> change)
    {
        if (!user.IsAuthenticated) return Result.Failure(CommonErrors.Unauthenticated);
        if (!user.IsInRole(RoleNames.Admin)) return Result.Failure(CommonErrors.Forbidden);
        if (id <= 0) return Result.Failure(OrderErrors.InvalidId);
        var order = await orders.GetForUpdateAsync(id, token);
        if (order is null) return Result.Failure(OrderErrors.NotFound(id));
        try
        {
            change(order, clock.GetUtcNow(), user.UserId!);
            await work.SaveChangesAsync(token);
            return Result.Success();
        }
        catch (DomainException e) { return Result.Failure(DomainErrorMapper.Map(e)); }
    }
}
