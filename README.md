# EshopMicroservices

> A .NET 10 microservices reference application demonstrating clean architecture, DDD, CQRS, event-driven messaging, and containerized deployment for an e-commerce platform.

**MIT License © 2026 Behdad Kardgar**

---

## Table of Contents

- [Project Overview](#project-overview)
- [System Architecture](#system-architecture)
- [Microservices Reference](#microservices-reference)
- [Technology Stack](#technology-stack)
- [Data Architecture](#data-architecture)
- [Running the Application](#running-the-application)
- [Design Patterns](#design-patterns)
- [Observability](#observability)
- [Project Structure](#project-structure)

---

## Project Overview

EshopMicroservices is a hands-on reference implementation that shows how to build a cloud-native e-commerce backend using the microservices architectural pattern on .NET 10.

**Goals:**
- Serve as a learning resource and reference for .NET microservices development
- Demonstrate best practices: clean architecture, vertical slice architecture, DDD
- Show realistic inter-service communication with both synchronous (gRPC) and asynchronous (RabbitMQ) messaging
- Provide a fully containerized development environment ready to run with a single command

---

## System Architecture

The system follows a loosely coupled microservices architecture. Each service owns its data, exposes its own API, and communicates with other services either synchronously via gRPC or asynchronously via a shared message bus (RabbitMQ).

```
                  ┌─────────────────────────────────────────┐
                  │              Client Apps                │
                  └───────────────────┬─────────────────────┘
                                      │ HTTP/HTTPS
  ┌───────────────────────────────────▼───────────────────────────────────────────┐
  │                                API Gateway (YARP)                             │
  │                           routing · rate limiting · reverse                   │
  │                                     proxy                                     │
  └────┬──────────┬──────────────────┬───────────────┬──────────┬────────────┬────┘
       │          │                  │               │          │            │
  ┌────▼────┐ ┌───▼───┐          ┌───▼─────┐    ┌────▼────┐┌────▼─────┐ ┌────▼──────┐
  │Catalog  │ │Basket │          │Discount │    │ Order-  ││ Order-   │ │ Identity  │
  │         │ │       │◄─────────────gRPC  │    │ command ││ query    │ │  (OIDC)   │
  └────┬────┘ └──┬────┘          └──────┬──┘    └───┬─────┘└─────┬────┘ └──────┬────┘
       │         │             RabbitMQ │           │            │             │ 
       │         │────────────[checkout]│───────────│────────────│             │
  ┌────▼───┐┌────▼──────┐ ┌─────┐ ┌─────▼──┐    ┌───▼───────┐┌───▼───────┐┌────▼──────┐
  │Post-   ││PostgreSQL │─│Redis│ │SQLite  │    │PostgreSQL ││PostgreSQL ││PostgreSQL │ 
  │greSQL  ││           │ └─────┘ └────────┘    │           ││           ││           │
  └────────┘└───────────┘                       └───────────┘└───────────┘└───────────┘

           Docker Compose (all containers)
```

### Architectural Layers

| Layer | Components |
|---|---|
| API Gateway | YARP reverse proxy — single entry point |
| Microservices | Catalog, Basket, Discount, Ordering, Identity |
| Messaging | RabbitMQ + MassTransit — async pub/sub |
| Data | Dedicated data store per service |
| Infrastructure | Docker Compose orchestration |

### Communication Patterns

**Synchronous (gRPC):** The Basket service calls the Discount service directly over gRPC to fetch real-time coupon data during cart operations. The response is needed immediately before proceeding.

**Asynchronous (RabbitMQ):** Here’s a clearer and more polished version:

Asynchronous (RabbitMQ):
When a basket is checked out, an HTTP request is sent to the Order Command service. The service persists the new order and publishes an OrderCreatedEvent to RabbitMQ. The Order Query service subscribes to this event and updates its read-optimized view of orders accordingly. This architecture decouples the command and query services, allowing them to scale independently. The trade-off is eventual consistency between the write model (Order Command) and the read model (Order Query).
---

## Microservices Reference

### Catalog Service
Manages the product catalogue. Read-heavy and optimised for query throughput.

- **Pattern:** Vertical Slice Architecture — each feature lives in its own folder with its own command/query, handler, and validator
- **CQRS:** Commands (writes) and queries (reads) are separated using MediatR
- **Database:** Marten document database over PostgreSQL for flexible product schemas
- **Validation:** FluentValidation pipeline behaviour validates all incoming requests

### Basket Service
Manages each user's shopping cart. Uses Redis because cart data is highly transient.

- **Redis cache:** Carts stored as JSON blobs with a configurable TTL
- **gRPC consumer:** Calls Discount service over gRPC before checkout to apply coupons
- **Cache-aside:** Cache is checked first; on miss, data is fetched from source and repopulated

### Discount Service
Simple coupon management service, consumed exclusively via gRPC.

- **gRPC server:** Exposes a Protobuf-defined service for coupon queries
- **Dapper + SQLite:** Lightweight data access; SQLite keeps the service self-contained
- **No public HTTP API:** Only accessible via gRPC from internal services

### Ordering Command Service
The most complex service — demonstrates full Domain-Driven Design and Clean Architecture.

- **Domain layer:** `Order` aggregate root, `OrderItem` entities, `Address` and `Money` value objects
- **Application layer:** CQRS handlers via MediatR; domain events raised and handled in-process and publishes events to RabbitMQ
- **Infrastructure layer:** EF Core with SQL Server; repository pattern abstracts persistence
- **Clean Architecture:** Dependency direction always flows inward (domain has zero dependencies)

### Ordering Query Service
Optimised for read performance with a materialized view of orders.
- **event processor:** Subscribes to order-related events from the command service and updates a read-optimised view in PostgreSQL
- **separate database:** Does not share the same database as the order command service, ensuring loose coupling
- **read-only API:** Exposes only query endpoints; no commands or mutations allowed
- **CQRS:** Commands handled by the command service; queries handled here, following the CQRS pattern
- **Materialized view:** The read model is a denormalised view of order data, optimised for query performance without joins
- **Eventual consistency:** The read model is updated asynchronously via events, so it may be slightly stale but is optimised for fast queries

### Identity Service
Handles authentication and authorisation across the system.

- Powered by **Duende IdentityServer**
- Issues JWT tokens via OpenID Connect / OAuth 2.0
- Integrates with ASP.NET Core Identity for user management

### API Gateway
Single entry point for all client requests.

- Built on **YARP** (Yet Another Reverse Proxy) by Microsoft
- Routing configured in `appsettings.json` — no code changes needed for new routes
- Handles rate limiting and can aggregate responses from multiple upstream services

---

## Technology Stack

| Category | Technology                                 | Purpose |
|---|--------------------------------------------|---|
| Web framework | ASP.NET Core 10 Minimal APIs               | HTTP API layer for all services |
| API Gateway | YARP                                       | Reverse proxy with JSON-based routing config |
| CQRS / Mediator | MediatR                                    | In-process command and query dispatching |
| ORM (Ordering) | Entity Framework Core 8                    | SQL Server access with migrations |
| DB Driver (Discount) | Dapper                                     | Micro-ORM for lightweight SQLite queries |
| Document DB | Marten + PostgreSQL                        | Schema-flexible document storage for catalogue |
| Cache | Redis (StackExchange.Redis)                | Distributed cache for basket data |
| Message broker | RabbitMQ                                   | Durable async queue for checkout events |
| Messaging library | MassTransit                                | Abstraction over RabbitMQ; handles retries and consumers |
| gRPC | Grpc.AspNetCore + Grpc.Net.Client          | Synchronous inter-service calls with Protobuf |
| Authentication | Duende IdentityServer                      | OpenID Connect / OAuth 2.0 token server |
| Validation | FluentValidation                           | Declarative request validation pipeline behaviour |
| API organisation | Carter                                     | Minimal API module pattern |
| Containerisation | Docker + Docker Compose                    | All services and databases run as containers |
| Observability | OpenTelemetry, Jaeger, Grafana, Prometheus | Tracing, metrics, and dashboards |
| Health checks | ASP.NET Core Health Checks                 | Liveness and readiness probes |

---

## Data Architecture

EshopMicroservices follows the **Database-per-Service** pattern. No service shares a database with another, ensuring loose coupling and independent deployability.

| Service | Data Store | Rationale |
|---|---|---|
| Catalog | PostgreSQL (Marten) | Document-oriented; products modelled as JSON |
| Basket | Redis | In-memory key-value; ideal for transient cart state |
| Discount | SQLite (Dapper) | Embedded relational; simple coupon table, self-contained |
| Ordering | SQL Server (EF Core) | Full ACID guarantees; relational order model with migrations |

### Event-Driven Synchronisation

When a user checks out, the system creates an order without the two services sharing a database:

1. Basket service publishes a `OrderCreatedEvent` event to RabbitMQ
2. Ordering service subscribes to `OrderCreatedEvent` events via a MassTransit consumer
3. The consumer maps the event payload to the Order domain model and persists it
4. The basket is cleared; order creation is designed to be idempotent in case of re-delivery

---

## Running the Application

### Prerequisites

- .NET 10 SDK
- Docker Desktop (or Docker Engine + Compose plugin)
- Git

### Quick Start

```bash
git clone https://github.com/behdad088/EshopMicroservices.git
cd EshopMicroservices
docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build
```

This builds all images and starts every service, database, RabbitMQ, and the observability stack.

### Service Endpoints (default ports)

| Service | URL |
|---|---|
| API Gateway | http://localhost:6064 |
| Catalog API | http://localhost:6000 |
| Basket API | http://localhost:6001 |
| Discount gRPC | http://localhost:6002 |
| Ordering API | http://localhost:6003 |
| RabbitMQ Management UI | http://localhost:15672 (guest / guest) |
| Jaeger (distributed tracing) | http://localhost:16686 |
| Prometheus | http://localhost:9090 |

---

## Design Patterns

### CQRS (Command Query Responsibility Segregation)
Commands (writes) and queries (reads) are handled by separate classes using MediatR. A FluentValidation pipeline behaviour validates all commands before they reach their handler.

### Domain-Driven Design
Applied in the Ordering service. `Order` is an aggregate root that enforces business invariants. `OrderItem` is an entity within the aggregate. `Address` and `Money` are value objects with no identity.

### Vertical Slice Architecture
Applied in the Catalog service. Each feature is a self-contained slice — endpoint, command/query, handler, and validator live together in one folder. No horizontal layering by type.

### Outbox Pattern & At-Least-Once Delivery

In a distributed system, publishing a message to a broker and saving data to a database are two separate operations — and either one can fail independently. Without a safety mechanism, this creates a window where your database is updated but the message is never sent (or vice versa), leaving the system in an inconsistent state.

The **Outbox pattern** solves this by treating message publishing as part of the same database transaction as the business operation. Instead of publishing directly to RabbitMQ, the service first writes the message to an `Outbox` table in its own database within the same transaction. A background process then reads from the Outbox and reliably forwards messages to the broker. If publishing fails, the background process retries — the message stays in the Outbox until delivery is confirmed.

**At-least-once delivery** is the guarantee that a message will be delivered a minimum of one time, but may be delivered more than once in failure scenarios (e.g. the consumer crashes after processing but before acknowledging the message). This is a deliberate trade-off — guaranteeing exactly-once delivery across a distributed system is extremely expensive, whereas at-least-once is achievable with straightforward retries.

In EshopMicroservices this is implemented via **MassTransit**, which provides:

- **Retry policies:** Failed message deliveries are automatically retried with configurable backoff intervals before being considered dead
- **Dead-letter queue (DLQ):** Messages that exhaust all retries are moved to a DLQ in RabbitMQ for manual inspection and replay, ensuring no message is silently lost
- **Idempotent consumers:** Because a message may arrive more than once, all consumers are written to be idempotent — processing the same `BasketCheckout` event twice produces the same result as processing it once (typically by checking whether an order with that ID already exists before creating a new one)

**Benefits of this approach:**

- **No silent data loss** — every published message is persisted before being sent; crashes cannot cause events to vanish
- **Resilience to transient failures** — network blips, broker restarts, or slow consumers are handled automatically via retries without developer intervention
- **Operational visibility** — the DLQ provides a clear signal when messages repeatedly fail, making bugs and integration issues easy to detect and diagnose
- **Decoupled reliability** — the publishing service does not need to know whether downstream consumers are available; it simply writes to the Outbox and moves on

### Health Checks
Every service exposes `/health` and `/health/ready` endpoints. Docker Compose uses these for dependency ordering; the API Gateway uses them for upstream availability.

---

## Observability

The system ships with a full observability stack powered by OpenTelemetry, providing distributed traces, metrics, and logs out of the box.

| Tool | Role |
|---|---|
| OpenTelemetry SDK | Instrumented in every service; exports traces and metrics via OTLP |
| Jaeger | Receives distributed traces; UI to inspect end-to-end request flows |
| Prometheus | Scrapes metrics endpoints from all services |

All observability infrastructure starts automatically as part of the Docker Compose stack.

---

## Project Structure

```
src/
├── Services/
│   ├── Catalog/           # Catalog.API
│   ├── Basket/            # Basket.API
│   ├── Discount/          # Discount.Grpc
│   ├── Ordering.Command/  # Ordering.API
│   │                      # Ordering.Application
│   │                      # Ordering.Domain
│   │                      # Ordering.Infrastructure
│   ├── Ordering.Query/    # Ordering.Query.API
│   │                      # Ordering.Query.EventProcessor
│   ├── Identity/          # Identity.API (Duende IdentityServer)
│   │
│   └── ApiGateways/           # YARP gateway configuration│
│
└── Shared/        # Shared exceptions, behaviours, messaging contracts

docker-compose.yml                  # Service definitions
docker-compose.override.yml         # Local port mappings and dev config
```

---

## License

MIT License © 2024 [Behdad Kardgar](https://github.com/behdad088)
