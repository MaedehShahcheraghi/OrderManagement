using FluentValidation;
using FluentValidation.Results;
using MediatR;
using OrderManagement.Application.Common.Results;

namespace OrderManagement.Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IResultFactory<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var failures = new List<ValidationFailure>();

        foreach (IValidator<TRequest> validator in validators)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResult = await validator.ValidateAsync(
                context,
                cancellationToken);

            failures.AddRange(validationResult.Errors);
        }

        if (failures.Count == 0)
        {
            return await next();
        }

        Dictionary<string, string[]> errors = failures
            .GroupBy(x => x.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(x => x.ErrorMessage)
                    .Distinct()
                    .ToArray());

        var error = new Error(
            "validation.failed",
            "One or more validation errors occurred.",
            ErrorType.Validation)
        {
            ValidationErrors = errors
        };

        return TResponse.Failure(error);
    }
}
