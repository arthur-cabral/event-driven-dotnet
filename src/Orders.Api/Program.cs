using Microsoft.EntityFrameworkCore;
using Orders.Application;
using Orders.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<KafkaOptions>(builder.Configuration.GetSection("Kafka"));
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("Orders")));
builder.Services.AddScoped<IOrderWriteStore, OrderWriteStore>();
builder.Services.AddScoped<IOrderCreator, CreateOrderHandler>();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
    await db.Database.EnsureCreatedAsync(); // Para produção, substitua por migrations versionadas.
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.MapControllers();
app.Run();
