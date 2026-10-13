namespace OrderManagement.Application.Common.Results;

public sealed record Error(
    string Code,
    string Message,
    ErrorType Type)
{
    public IReadOnlyDictionary<string, string[]>?
        ValidationErrors { get; init; }
}
