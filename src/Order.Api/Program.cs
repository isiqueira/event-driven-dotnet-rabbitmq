using Microsoft.EntityFrameworkCore;
using Order.Api.Data;
using Order.Api.Endpoints;
using Order.Api.Messaging;
using Order.Api.Middleware;
using Shared.Data.Abstractions;
using Shared.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("OrdersDb"),
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_orders")
    )
);

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName)
);

builder.Services.AddSingleton<RabbitMqInitializer>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddSingleton<IOrderEventPublisher, RabbitMqOrderEventPublisher>();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}))
.WithName("GetHealth")
.WithTags("Health");

app.MapOrderEndpoints();

await OrdersDbInitializer.InitializeAsync(app.Services);
await app.Services.GetRequiredService<RabbitMqInitializer>().InitializeAsync();

app.Run();

public partial class Program;
