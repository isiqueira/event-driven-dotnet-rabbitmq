# Event-driven .NET with RabbitMQ

Event-driven architecture sample using .NET 10, RabbitMQ, PostgreSQL, retries,
dead-letter queues, idempotency, and correlation IDs.

This repository demonstrates a small, production-inspired backend architecture
using asynchronous messaging between an API, an order worker, and an inventory
worker.

## Purpose

The goal of this project is to demonstrate practical event-driven architecture
patterns commonly used in backend and enterprise systems.

It focuses on:

- REST API with .NET 10
- RabbitMQ message publishing and consuming with `RabbitMQ.Client`
- PostgreSQL persistence with EF Core and Npgsql
- Integration events
- Correlation IDs
- Idempotent consumers
- Inventory reservation and deduction
- Retry handling
- Dead-letter queues
- Dockerized local infrastructure
- Clear architecture documentation

## Technology Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 |
| API | ASP.NET Core minimal APIs |
| Workers | .NET Worker Service |
| Messaging | RabbitMQ |
| Persistence | PostgreSQL with EF Core |
| Tests | xUnit |
| Local infrastructure | Docker Compose |

## Business Scenario

The sample domain is order processing with inventory reservation.

When a new order is created, the API stores it in PostgreSQL and publishes an
`OrderCreated` event to RabbitMQ.

The inventory worker reserves stock for the order and publishes either
`InventoryReserved` or `InventoryReservationFailed`.

The order worker only processes the order after `InventoryReserved`. Once the
order is processed, the inventory worker deducts the previously reserved
quantity and publishes `InventoryDeducted`.

The order worker marks the order as fulfilled only after `InventoryDeducted`.

This simple flow is enough to demonstrate real-world messaging concerns such as
duplicate messages, retries, failures, and observability.

## Architecture Overview

```text
Client
  |
  | POST /orders
  v
Order.Api
  |
  | Save order as PendingInventoryReservation
  v
PostgreSQL
  |
  | Publish OrderCreated
  v
RabbitMQ
  |
  | Consume OrderCreated
  v
Inventory.Worker
  |
  | Check idempotency
  | Reserve inventory
  | Publish InventoryReserved
  v
RabbitMQ
  |
  | Consume InventoryReserved
  v
Order.Worker
  |
  | Process order
  | Publish OrderProcessed
  v
RabbitMQ
  |
  | Consume OrderProcessed
  v
Inventory.Worker
  |
  | Deduct reserved inventory
  | Publish InventoryDeducted
  v
RabbitMQ
  |
  | Consume InventoryDeducted
  v
Order.Worker
  |
  | Mark order as Fulfilled
  | Publish OrderFulfilled
  v
processed_messages
```

## Projects

| Project | Description |
| --- | --- |
| `src/Order.Api` | HTTP API that receives orders and publishes integration events |
| `src/Inventory.Worker` | Background worker that reserves and deducts inventory |
| `src/Order.Worker` | Background worker that reacts to inventory events and advances order status |
| `src/Shared` | Shared event and messaging contracts |
| `src/Shared.Models` | Shared sample domain models |
| `src/Shared.Data` | Reusable EF Core entity configuration |
| `tests/Inventory.Worker.Tests` | Unit tests for inventory reservation and deduction behavior |
| `tests/Order.Api.Tests` | Unit tests for API domain and validation behavior |
| `tests/Order.Worker.Tests` | Unit tests for order workflow and idempotency behavior |

`Shared.Models`, `Shared.Data`, and the specific repository abstractions keep
this educational sample small and readable. In a strict microservices
architecture, each service should own its domain model and persistence schema
independently. This repository is a modular event-driven architecture sample,
not a strict microservices template.

## Run Locally

Start infrastructure:

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project src/Order.Api
```

Run the inventory worker in another terminal:

```bash
dotnet run --project src/Inventory.Worker
```

Run the order worker in another terminal:

```bash
dotnet run --project src/Order.Worker
```

Create an order:

```bash
curl -X POST http://localhost:5282/orders \
  -H "Content-Type: application/json" \
  -H "X-Correlation-ID: demo-correlation-001" \
  -d "{\"customerId\":\"customer-001\",\"items\":[{\"sku\":\"SKU-001\",\"quantity\":2,\"unitPrice\":100.00}]}"
```

Useful local URLs:

| Service | URL | Credentials |
| --- | --- | --- |
| API Swagger UI | `http://localhost:5282/swagger` | Not required |
| RabbitMQ management | `http://localhost:15672` | `app` / `app` |

## Validation

Build the solution:

```bash
dotnet build
```

Run the tests:

```bash
dotnet test
```

Validate Docker Compose:

```bash
docker compose config
```
