using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Results;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Repositories;
namespace OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;
public sealed class ConfirmOrderCommandHandler(
    IOrderWriteRepository orders, IUnitOfWork unitOfWork,
    ICurrentUser currentUser, TimeProvider clock, IInventoryRepository inventory)
    : OrderMutationHandler<ConfirmOrderCommand>(orders, unitOfWork, currentUser, clock)
{
    protected override void Apply(Order order, DateTimeOffset now, string actorId) =>
        order.Confirm(now, actorId);

    protected override async Task<Error?> BeforeSaveAsync(Order order, CancellationToken token)
    {
        foreach (var item in order.Items.OrderBy(x => x.ProductId))
            if (!await inventory.TryDecreaseAsync(item.ProductId, item.Quantity, token))
                return OrderErrors.InsufficientStock(item.ProductId);
        return null;
    }
}

