namespace OrderManagement.Application.Common.Results;

public sealed class Result<T> : Result
{
    private Result(T? value, Error? error)
        : base(error)
    {
        Value = value;
    }

    public T Value => IsSuccess
        ? field!
        : throw new InvalidOperationException(
            "A failed result has no value.");

    public static Result<T> Success(T value) =>
        new(value, null);

    public new static Result<T> Failure(Error error) =>
        new(
            default,
            error ?? throw new ArgumentNullException(nameof(error)));
}
