using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace OrderManagement.Api.Common.Extensions;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException &&
            context.RequestAborted.IsCancellationRequested)
        {
            return false;
        }

        logger.LogError(
            exception,
            "Unhandled exception. TraceId: {TraceId}",
            context.TraceIdentifier);

        if (context.Response.HasStarted)
            return false;

        context.Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal server error",
            Detail = "An unexpected error occurred.",
            Extensions =
            {
                ["code"] = "server.unexpected",
                ["traceId"] = context.TraceIdentifier
            }
        };

        await context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
