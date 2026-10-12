using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Common.Errors;

public static class OrderErrors
{
    public static readonly Error InvalidId = new(
        "orders.invalid_id",
        "Order ID must be positive.",
        ErrorType.Validation);

    public static Error NotFound(long id) => new(
        "orders.not_found",
        $"Order {id} was not found.",
        ErrorType.NotFound);

    public static Error InsufficientStock(long productId) => new(
        "orders.insufficient_stock",
        $"Insufficient stock for product {productId}.",
        ErrorType.Conflict);

    public static readonly Error ConcurrentModification = new(
        "orders.concurrent_modification",
        "The order changed concurrently. Reload it and try again.",
        ErrorType.Conflict);
}
