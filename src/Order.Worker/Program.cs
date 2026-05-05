using Microsoft.EntityFrameworkCore;
using Order.Worker;
using Order.Worker.Data;
using Order.Worker.Messaging;
using Order.Worker.Services;
using Shared.Data.Abstractions;
using Shared.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<OrderWorkerDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WorkerDb"),
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_worker")));

builder.Services.AddDbContext<OrderWorkflowDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName)
);

builder.Services.AddScoped<ProcessedMessageStore>();
builder.Services.AddScoped<IOrderRepository, EfOrderRepository>();
builder.Services.AddScoped<IProcessedMessageRepository, EfProcessedMessageRepository>();
builder.Services.AddScoped<OrderProcessingService>();
builder.Services.AddScoped<OrderWorkflowService>();
builder.Services.AddScoped<InventoryReservedMessageHandler>();
builder.Services.AddScoped<InventoryReservationFailedMessageHandler>();
builder.Services.AddScoped<InventoryDeductedMessageHandler>();
builder.Services.AddSingleton<IOrderWorkflowEventPublisher, RabbitMqOrderWorkflowEventPublisher>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await WorkerDbInitializer.InitializeAsync(host.Services);
await host.RunAsync();
