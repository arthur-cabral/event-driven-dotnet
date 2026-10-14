using Microsoft.EntityFrameworkCore;
using Orders.Domain;

namespace Orders.Infrastructure;

public sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(b => { b.ToTable("orders"); b.HasKey(x => x.Id); b.Property(x => x.Description).HasMaxLength(200).IsRequired(); b.Property(x => x.CreatedAtUtc).IsRequired(); });
        modelBuilder.Entity<OutboxMessage>(b => { b.ToTable("outbox_messages"); b.HasKey(x => x.Id); b.Property(x => x.Type).HasMaxLength(200).IsRequired(); b.Property(x => x.Payload).IsRequired(); b.HasIndex(x => x.PublishedAtUtc); });
    }
}
