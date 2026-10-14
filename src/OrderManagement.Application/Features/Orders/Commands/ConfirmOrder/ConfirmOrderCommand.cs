namespace OrderManagement.Application.Features.Orders.Commands.ConfirmOrder;
public sealed record ConfirmOrderCommand(long OrderId) : IOrderIdRequest;

