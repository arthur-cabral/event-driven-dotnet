using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Orders.Application;
using Orders.Domain;

namespace Orders.Infrastructure;

public sealed class OrderWriteStore(OrdersDbContext db) : IOrderWriteStore
{
    public async Task SaveOrderAndOutboxAsync(Order order, CancellationToken cancellationToken)
    {
        var eventPayload = JsonSerializer.Serialize(new OrderCreatedEvent(order.Id, order.Description, order.CreatedAtUtc));
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        db.Orders.Add(order);
        db.OutboxMessages.Add(new OutboxMessage { Id = Guid.NewGuid(), Type = "OrderCreated", Payload = eventPayload, OccurredAtUtc = DateTimeOffset.UtcNow });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}

public sealed record OrderCreatedEvent(Guid OrderId, string Description, DateTimeOffset CreatedAtUtc);
