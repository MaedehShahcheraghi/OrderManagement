namespace OrderManagement.Application.Features.Orders.Models;
public sealed record OrderItemDto(long ProductId, int Quantity, decimal UnitPrice)
{
    public decimal LineTotal => Quantity * UnitPrice;
}
