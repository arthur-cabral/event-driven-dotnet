namespace Orders.Infrastructure;

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public int Attempts { get; set; }
    public string? LastError { get; set; }
}
