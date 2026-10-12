using MediatR;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Common.Errors;
using OrderManagement.Application.Common.Exceptions;
using OrderManagement.Application.Common.Results;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Repositories;

namespace OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;

public sealed class ConfirmOrderCommandHandler(
    IOrderWriteRepository orders,
    IInventoryRepository inventory,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider timeProvider)
    : IRequestHandler<ConfirmOrderCommand, Result>
{
    public async Task<Result> Handle(
        ConfirmOrderCommand request,
        CancellationToken cancellationToken)
    {
        string? actorId = currentUser.UserId;

        if (string.IsNullOrWhiteSpace(actorId))
        {
            return Result.Failure(
                CommonErrors.Unauthenticated);
        }

        if (request.OrderId <= 0)
        {
            return Result.Failure(
                OrderErrors.InvalidId);
        }

        try
        {
            await using var transaction =
                await unitOfWork.BeginTransactionAsync(
                    cancellationToken);

            Order? order =
                await orders.GetForUpdateAsync(
                    request.OrderId,
                    cancellationToken);

            if (order is null)
            {
                return Result.Failure(
                    OrderErrors.NotFound(request.OrderId));
            }

            order.Confirm(
                timeProvider.GetUtcNow(),
                actorId);

            foreach (OrderItem item in order.Items
                         .OrderBy(x => x.ProductId))
            {
                bool decreased =
                    await inventory.TryDecreaseAsync(
                        item.ProductId,
                        item.Quantity,
                        cancellationToken);

                if (!decreased)
                {
                    return Result.Failure(
                        OrderErrors.InsufficientStock(
                            item.ProductId));
                }
            }

            await unitOfWork.SaveChangesAsync(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);

            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(
                DomainErrorMapper.Map(exception));
        }
        catch (PersistenceConcurrencyException)
        {
            return Result.Failure(
                OrderErrors.ConcurrentModification);
        }
    }
}
