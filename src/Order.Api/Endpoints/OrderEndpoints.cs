using Order.Api.Contracts;
using Order.Api.Messaging;
using Order.Api.Middleware;
using Order.Api.Services;
using Shared.Data.Abstractions;
using Shared.Events;
using Shared.Models.Orders;
using OrderEntity = Shared.Models.Orders.Order;

namespace Order.Api.Endpoints;

public static class OrderEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/orders").WithTags("Orders");

        group.MapPost("", CreateOrderAsync)
            .WithName("CreateOrder")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapGet("", GetOrdersAsync)
            .WithName("GetOrders")
            .Produces<IReadOnlyCollection<OrderResponse>>();

        group.MapGet("{id:guid}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .Produces<OrderResponse>()
            .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }

    private static async Task<IResult> CreateOrderAsync(
        CreateOrderRequest? request,
        HttpContext httpContext,
        IOrderRepository orderRepository,
        IOrderEventPublisher eventPublisher,
        ILoggerFactory loggerFactory,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("Order.Api.Endpoints.OrderEndpoints");
        var validationErrors = CreateOrderRequestValidator.Validate(request);
        if (validationErrors.Count > 0)
        {
            return Results.ValidationProblem(validationErrors);
        }

        var items = request!.Items!
            .Select(item => new OrderItemInput(item.Sku!, item.Quantity, item.UnitPrice))
            .ToArray();

        var order = OrderEntity.Create(request.CustomerId!, items, DateTimeOffset.UtcNow);
        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var correlationId = CorrelationIdMiddleware.GetCorrelationId(httpContext);
        var integrationEvent = new OrderCreatedEvent(
            Guid.NewGuid(),
            OrderCreatedEvent.Name,
            DateTimeOffset.UtcNow,
            correlationId,
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            order.Items
                .Select(item => new OrderCreatedItem(item.Sku, item.Quantity))
                .ToArray());

        await eventPublisher.PublishAsync(integrationEvent, cancellationToken);

        logger.LogInformation(
            "Created order {OrderId} for customer {CustomerId} with total {TotalAmount} and correlation ID {CorrelationId}",
            order.Id,
            order.CustomerId,
            order.TotalAmount,
            correlationId);

        return Results.Created($"/orders/{order.Id}", OrderResponse.From(order, correlationId));
    }

    private static async Task<IResult> GetOrdersAsync(
        IOrderRepository orderRepository,
        CancellationToken cancellationToken)
    {
        var orders = await orderRepository.ListAsync(cancellationToken);

        return Results.Ok(orders.Select(order => OrderResponse.From(order)).ToArray());
    }

    private static async Task<IResult> GetOrderByIdAsync(
        Guid id,
        IOrderRepository orderRepository,
        CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetByIdAsync(id, cancellationToken);

        return order is null
            ? Results.NotFound()
            : Results.Ok(OrderResponse.From(order));
    }
}
