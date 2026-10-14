namespace Orders.Infrastructure;

public sealed class KafkaOptions
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string Topic { get; set; } = "ordersCreatedV1";
    public string ConsumerGroup { get; set; } = "orders-worker-v1";
}
