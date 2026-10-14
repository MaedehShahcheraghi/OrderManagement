using MediatR;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Errors;
using OrderManagement.Application.Common.Results;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(
    IOrderWriteRepository orders, IUnitOfWork unitOfWork,
    ICurrentUser currentUser, TimeProvider clock, IInventoryRepository inventory)
    : IRequestHandler<ConfirmOrderCommand, Result>
{
    public async Task<Result> Handle(ConfirmOrderCommand request, CancellationToken token)
    {
        if (!currentUser.IsAuthenticated) return Result.Failure(CommonErrors.Unauthenticated);
        if (!currentUser.IsInRole("Admin")) return Result.Failure(CommonErrors.Forbidden);
        if (request.OrderId <= 0) return Result.Failure(OrderErrors.InvalidId);

        await using var transaction = await unitOfWork.BeginTransactionAsync(token);
        var order = await orders.GetForUpdateAsync(request.OrderId, token);
        if (order is null) return Result.Failure(OrderErrors.NotFound(request.OrderId));

        try
        {
            order.Confirm(clock.GetUtcNow(), currentUser.UserId!);
            foreach (var item in order.Items.OrderBy(x => x.ProductId))
            {
                if (!await inventory.TryDecreaseAsync(item.ProductId, item.Quantity, token))
                    return Result.Failure(OrderErrors.InsufficientStock(item.ProductId));
            }
            await unitOfWork.SaveChangesAsync(token);
            await transaction.CommitAsync(token);
            return Result.Success();
        }
        catch (DomainException ex) { return Result.Failure(DomainErrorMapper.Map(ex)); }
    }
}
