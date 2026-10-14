using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orders.Infrastructure;

public sealed class OrdersEventConsumer(IOptions<KafkaOptions> options, ILogger<OrdersEventConsumer> logger) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.Run(() => Consume(stoppingToken), stoppingToken);

    private void Consume(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig { BootstrapServers = options.Value.BootstrapServers, GroupId = options.Value.ConsumerGroup, AutoOffsetReset = AutoOffsetReset.Earliest, EnableAutoCommit = false };
        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(options.Value.Topic);
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = consumer.Consume(stoppingToken);
                    // Substitua pelo handler de negócio. Não marque como processado antes de concluir o trabalho.
                    logger.LogInformation("Consumed event key={Key}, partition={Partition}, offset={Offset}, payload={Payload}", result.Message.Key, result.Partition.Value, result.Offset.Value, result.Message.Value);
                    consumer.Commit(result);
                }
                catch (ConsumeException ex) { logger.LogError(ex, "Kafka consume error"); }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally { consumer.Close(); }
    }
}
