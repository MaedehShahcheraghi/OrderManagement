namespace OrderManagement.Application.Common.Results;

public interface IResultFactory<TSelf>
    where TSelf : IResultFactory<TSelf>
{
    static abstract TSelf Failure(Error error);
}
