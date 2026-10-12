using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Common.Errors;

public static class CommonErrors
{
    public static readonly Error Unauthenticated = new(
        "auth.unauthenticated",
        "Authentication is required.",
        ErrorType.Unauthorized);

    public static readonly Error Forbidden = new(
        "auth.forbidden",
        "You do not have permission for this operation.",
        ErrorType.Forbidden);
}
