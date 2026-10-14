using Orders.Domain;

namespace Orders.Application;

public sealed record CreateOrderCommand(string Description);
public sealed record OrderCreatedResult(Guid Id, string Description, DateTimeOffset CreatedAtUtc);

public interface IOrderCreator
{
    Task<OrderCreatedResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
}

public interface IOrderWriteStore
{
    Task SaveOrderAndOutboxAsync(Order order, CancellationToken cancellationToken);
}

public sealed class CreateOrderHandler(IOrderWriteStore store) : IOrderCreator
{
    public async Task<OrderCreatedResult> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var order = Order.Create(command.Description);
        await store.SaveOrderAndOutboxAsync(order, cancellationToken);
        return new OrderCreatedResult(order.Id, order.Description, order.CreatedAtUtc);
    }
}
