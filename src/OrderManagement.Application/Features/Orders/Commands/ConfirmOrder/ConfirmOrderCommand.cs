using MediatR;
using OrderManagement.Application.Common.Results;
namespace OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;
public sealed record ConfirmOrderCommand(long OrderId) : IRequest<Result>;
