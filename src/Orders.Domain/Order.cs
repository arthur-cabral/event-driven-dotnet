namespace Orders.Domain;

public sealed class Order
{
    private Order() { }
    public Guid Id { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Order Create(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description is required.", nameof(description));
        if (description.Length > 200) throw new ArgumentException("Description must be at most 200 characters.", nameof(description));
        return new Order { Id = Guid.NewGuid(), Description = description.Trim(), CreatedAtUtc = DateTimeOffset.UtcNow };
    }
}
