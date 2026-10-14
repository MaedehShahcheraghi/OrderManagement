using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Common.Exceptions;

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

        bool concurrency = exception is PersistenceConcurrencyException;
        int status = concurrency
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status500InternalServerError;
        context.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = concurrency ? "Conflict" : "Internal server error",
            Detail = concurrency
                ? "The order changed concurrently. Reload it and try again."
                : "An unexpected error occurred.",
            Extensions =
            {
                ["code"] = concurrency
                    ? "orders.concurrent_modification"
                    : "server.unexpected",
                ["traceId"] = context.TraceIdentifier
            }
        };

        await context.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }
}
