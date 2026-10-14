using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orders.Infrastructure;

public sealed class OutboxPublisher(IServiceScopeFactory scopeFactory, IOptions<KafkaOptions> options, ILogger<OutboxPublisher> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var producerConfig = new ProducerConfig { BootstrapServers = options.Value.BootstrapServers, EnableIdempotence = true };
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
                var pending = await db.OutboxMessages.Where(x => x.PublishedAtUtc == null).OrderBy(x => x.OccurredAtUtc).Take(50).ToListAsync(stoppingToken);
                if (pending.Count == 0) { await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken); continue; }
                foreach (var message in pending)
                {
                    try
                    {
                        await producer.ProduceAsync(options.Value.Topic, new Message<string, string> { Key = message.Id.ToString(), Value = message.Payload }, stoppingToken);
                        message.PublishedAtUtc = DateTimeOffset.UtcNow; message.LastError = null;
                    }
                    catch (Exception ex)
                    {
                        message.Attempts++; message.LastError = ex.Message; logger.LogError(ex, "Failed to publish outbox message {MessageId}", message.Id);
                        break;
                    }
                }
                await db.SaveChangesAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex) { logger.LogError(ex, "Outbox publisher loop failed"); await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken); }
        }
    }
}
