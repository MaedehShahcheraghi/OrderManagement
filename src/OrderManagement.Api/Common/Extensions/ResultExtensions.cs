using OrderManagement.Application.Common.Results;
namespace OrderManagement.Api.Common;
public static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result, HttpContext context) =>
        result.IsSuccess ? Results.NoContent() : ToProblem(result.Error!, context);
    public static IResult ToHttpResult<T>(this Result<T> result, HttpContext context) =>
        result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error!, context);
    private static IResult ToProblem(Error error, HttpContext context)
    {
        int status = error.Type switch
        {
            ErrorType.Validation => 400, ErrorType.NotFound => 404,
            ErrorType.Unauthorized => 401, ErrorType.Forbidden => 403,
            ErrorType.Conflict => 409, _ => 500
        };
        if (status == 401) context.Response.Headers.WWWAuthenticate = "Bearer";
        var extensions = new Dictionary<string, object?>
            { ["code"] = error.Code, ["traceId"] = context.TraceIdentifier };
        if (error.ValidationErrors is not null)
            extensions["errors"] = error.ValidationErrors;
        return Results.Problem(statusCode: status, title: error.Type.ToString(),
            detail: error.Message, extensions: extensions);
    }
}
