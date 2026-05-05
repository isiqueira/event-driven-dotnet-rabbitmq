# Event-driven .NET with RabbitMQ

Event-driven architecture sample using .NET 10, RabbitMQ, PostgreSQL, retries,
dead-letter queues, idempotency, and correlation IDs.

This repository demonstrates a small, production-inspired backend architecture
using asynchronous messaging between an API and a background worker.

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
- Retry handling
- Dead-letter queues
- Dockerized local infrastructure
- Clear architecture documentation

## Technology Stack

| Area | Technology |
| --- | --- |
| Runtime | .NET 10 |
| API | ASP.NET Core minimal APIs |
| Worker | .NET Worker Service |
| Messaging | RabbitMQ |
| Persistence | PostgreSQL with EF Core |
| Tests | xUnit |
| Local infrastructure | Docker Compose |

## Business Scenario

The sample domain is order processing.

When a new order is created, the API stores it in PostgreSQL and publishes an
`OrderCreated` event to RabbitMQ.

A worker consumes this event asynchronously and processes it.

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
  | Save order
  v
PostgreSQL
  |
  | Publish OrderCreated
  v
RabbitMQ
  |
  | Consume event
  v
Order.Worker
  |
  | Check idempotency
  | Process order
  v
processed_messages
```

## Projects

| Project | Description |
| --- | --- |
| `src/Order.Api` | HTTP API that receives orders and publishes integration events |
| `src/Order.Worker` | Background worker that consumes order events |
| `src/Shared` | Shared event and messaging contracts |
| `tests/Order.Api.Tests` | Unit tests for API domain and validation behavior |
| `tests/Order.Worker.Tests` | Unit tests for worker idempotency behavior |

## Run Locally

Start infrastructure:

```bash
docker compose up -d
```

Run the API:

```bash
dotnet run --project src/Order.Api
```

Run the worker in another terminal:

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
