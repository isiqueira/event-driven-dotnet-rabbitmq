using Inventory.Worker;
using Inventory.Worker.Data;
using Inventory.Worker.Messaging;
using Inventory.Worker.Services;
using Microsoft.EntityFrameworkCore;
using Shared.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("InventoryDb"),
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_inventory")));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName)
);

builder.Services.AddScoped<ProcessedMessageStore>();
builder.Services.AddScoped<InventoryReservationService>();
builder.Services.AddScoped<OrderCreatedMessageHandler>();
builder.Services.AddScoped<OrderProcessedMessageHandler>();
builder.Services.AddSingleton<IInventoryEventPublisher, RabbitMqInventoryEventPublisher>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await InventoryDbInitializer.InitializeAsync(host.Services);
await host.RunAsync();
