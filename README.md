# Event-driven .NET with RabbitMQ

Event-driven architecture sample using .NET 8, RabbitMQ, PostgreSQL, retries, dead-letter queue, idempotency and correlation IDs.

This repository demonstrates how to build a small but production-inspired backend architecture using asynchronous messaging between an API and a background worker.

## Purpose

The goal of this project is to demonstrate practical event-driven architecture patterns commonly used in enterprise systems.

It focuses on:

- REST API with .NET 8
- RabbitMQ message publishing and consuming
- PostgreSQL persistence
- Integration events
- Correlation IDs
- Idempotent consumers
- Retry handling
- Dead-letter queue
- Dockerized local environment
- Clear architecture documentation

## Business scenario

The sample domain is order processing.

When a new order is created, the API stores it in PostgreSQL and publishes an `OrderCreated` event to RabbitMQ. A worker consumes this event asynchronously and processes it.

This simple flow is enough to demonstrate real-world messaging concerns such as duplicate messages, retries, failures and observability.

## Architecture overview

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
ProcessedMessages
