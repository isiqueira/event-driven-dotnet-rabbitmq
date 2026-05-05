using Microsoft.EntityFrameworkCore;
using Order.Worker;
using Order.Worker.Data;
using Order.Worker.Services;
using Shared.Messaging;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<WorkerDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("WorkerDb"),
        npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_worker")));

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName)
);

builder.Services.AddScoped<ProcessedMessageStore>();
builder.Services.AddScoped<OrderProcessingService>();
builder.Services.AddScoped<OrderCreatedMessageHandler>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

await WorkerDbInitializer.InitializeAsync(host.Services);
await host.RunAsync();
