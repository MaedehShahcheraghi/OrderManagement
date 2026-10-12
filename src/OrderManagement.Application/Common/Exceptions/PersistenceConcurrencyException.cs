namespace OrderManagement.Application.Common.Exceptions;

public sealed class PersistenceConcurrencyException(
    Exception innerException)
    : Exception(
        "A persistence concurrency conflict occurred.",
        innerException);
