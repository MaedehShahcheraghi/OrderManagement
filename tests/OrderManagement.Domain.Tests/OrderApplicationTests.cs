using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Features.Orders.Commands;
using OrderManagement.Domain.Entities.Customers;
using OrderManagement.Domain.Entities.Orders;
using OrderManagement.Domain.Entities.Products;
using OrderManagement.Domain.Repositories;
using Xunit;

namespace OrderManagement.Domain.Tests;

public sealed class OrderApplicationTests
{
    [Fact]
    public async Task Create_order_uses_trusted_product_price()
    {
        var write = new FakeOrders();
        var handler = new CreateOrderHandler(
            new FakeCustomers(), new FakeProducts(), write, new FakeUnitOfWork(),
            new FakeUser(1), TimeProvider.System);

        var result = await handler.Handle(new CreateOrderCommand(1,
            [new NewOrderItem(10, 2), new NewOrderItem(10, 3)]), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(write.LastOrder);
        Assert.Single(write.LastOrder.Items);
        Assert.Equal(125m, write.LastOrder.TotalAmount);
        Assert.Equal(5, write.LastOrder.Items.Single().Quantity);
    }

    [Fact]
    public async Task User_cannot_create_order_for_another_customer()
    {
        var write = new FakeOrders();
        var handler = new CreateOrderHandler(
            new FakeCustomers(), new FakeProducts(), write, new FakeUnitOfWork(),
            new FakeUser(2), TimeProvider.System);

        var result = await handler.Handle(new CreateOrderCommand(1,
            [new NewOrderItem(10, 1)]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("auth.forbidden", result.Error?.Code);
        Assert.Null(write.LastOrder);
    }

    [Fact]
    public async Task Unknown_products_are_rejected_before_persistence()
    {
        var write = new FakeOrders();
        var handler = new CreateOrderHandler(
            new FakeCustomers(), new FakeProducts(), write, new FakeUnitOfWork(),
            new FakeUser(1), TimeProvider.System);

        var result = await handler.Handle(new CreateOrderCommand(1,
            [new NewOrderItem(999, 1)]), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("orders.unknown_product", result.Error?.Code);
        Assert.Null(write.LastOrder);
    }

    private sealed class FakeUser(long customerId) : ICurrentUser
    {
        public string? UserId => "unit-tester";
        public long? CustomerId => customerId;
        public bool IsAuthenticated => true;
        public bool IsInRole(string role) => false;
    }

    private sealed class FakeOrders : IOrderWriteRepository
    {
        public Order? LastOrder { get; private set; }
        public void Add(Order order) => LastOrder = order;
        public void AddRange(IEnumerable<Order> orders) => LastOrder = orders.LastOrDefault();
        public Task<Order?> GetForUpdateAsync(long id, CancellationToken token = default) =>
            Task.FromResult(LastOrder);
    }

    private sealed class FakeCustomers : ICustomerRepository
    {
        public Task<bool> ExistsAsync(long id, CancellationToken token = default) =>
            Task.FromResult(id == 1);
        public Task<IReadOnlySet<long>> GetExistingIdsAsync(
            IReadOnlyCollection<long> ids, CancellationToken token = default) =>
            Task.FromResult<IReadOnlySet<long>>(ids.Where(x => x == 1).ToHashSet());
    }

    private sealed class FakeProducts : IProductRepository
    {
        public Task<IReadOnlyList<Product>> GetByIdsAsync(
            IReadOnlyCollection<long> ids, CancellationToken token = default)
        {
            if (!ids.Contains(10))
                return Task.FromResult<IReadOnlyList<Product>>([]);

            var product = Product.Create("TEST-10", "Product 10", 25m);
            typeof(Product).GetProperty(nameof(Product.Id))!.SetValue(product, 10L);
            return Task.FromResult<IReadOnlyList<Product>>([product]);
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken token = default) =>
            Task.FromResult(1);
        public Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken token = default) =>
            Task.FromResult<IUnitOfWorkTransaction>(new FakeTransaction());
    }

    private sealed class FakeTransaction : IUnitOfWorkTransaction
    {
        public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
        public Task RollbackAsync(CancellationToken token = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
