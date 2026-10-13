using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Common.Errors;
public static class AuthErrors
{
    public static readonly Error InvalidInput = new(
        "auth.invalid_input",
        "Email and password are required.",
        ErrorType.Validation);

    public static readonly Error InvalidCredentials = new(
        "auth.invalid_credentials",
        "Invalid email or password.",
        ErrorType.Unauthorized);
}
