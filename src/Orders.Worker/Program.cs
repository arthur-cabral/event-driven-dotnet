using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Orders.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Orders")));
builder.Services.AddHostedService<OutboxPublisher>();
builder.Services.AddHostedService<OrdersEventConsumer>();
var host = builder.Build();
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.EnsureCreatedAsync();
}
await host.RunAsync();
