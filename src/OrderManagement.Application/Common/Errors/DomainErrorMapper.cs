using OrderManagement.Application.Common.Results;
using OrderManagement.Domain.Common;

namespace OrderManagement.Application.Common.Errors;
public static class DomainErrorMapper
{
    public static Error Map(DomainException exception)
    {
        ErrorType type = exception.Code switch
        {
            "order.invalid_status_transition" or
                "order.already_deleted" or
                "inventory.insufficient_stock"
                => ErrorType.Conflict,

            _ => ErrorType.Validation
        };

        return new Error(
            exception.Code,
            exception.Message,
            type);
    }
}
