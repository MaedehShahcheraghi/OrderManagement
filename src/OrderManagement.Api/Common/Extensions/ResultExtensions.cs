using OrderManagement.Application.Common.Results;

namespace OrderManagement.Api.Common.Extensions;

public static class ResultExtensions
{
    public static IResult ToHttpResult(
        this Result result,
        HttpContext context) =>
        result.IsSuccess
            ? Results.NoContent()
            : ToProblem(result.Error!, context);

    public static IResult ToHttpResult<T>(
        this Result<T> result,
        HttpContext context) =>
        result.IsSuccess
            ? Results.Ok(result.Value)
            : ToProblem(result.Error!, context);

    private static IResult ToProblem(
        Error error,
        HttpContext context)
    {
        int status = error.Type switch
        {
            ErrorType.Validation =>
                StatusCodes.Status400BadRequest,

            ErrorType.NotFound =>
                StatusCodes.Status404NotFound,

            ErrorType.Unauthorized =>
                StatusCodes.Status401Unauthorized,

            ErrorType.Forbidden =>
                StatusCodes.Status403Forbidden,

            ErrorType.Conflict =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

        if (status == StatusCodes.Status401Unauthorized)
        {
            context.Response.Headers.WWWAuthenticate = "Bearer";
        }

        return Results.Problem(
            statusCode: status,
            title: error.Type.ToString(),
            detail: error.Message,
            extensions: new Dictionary<string, object?>
            {
                ["code"] = error.Code,
                ["traceId"] = context.TraceIdentifier
            });
    }
}
