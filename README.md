# Order Management Service

A simple Order Management API built with **.NET 10** and **Clean Architecture**.

## Project Status

### Completed
- Created the solution with 5 projects: Domain, Application, Infrastructure, API, and Worker.
- Implemented Order, OrderItem, Customer, Product, and Inventory models.
- Added order status management: Pending, Confirmed, Shipped, and Delivered.
- Added basic business rules, validation, and soft deletion.
- Added a health check endpoint.
- Verified that the API and Worker run successfully.

### TODO
- SQL Server and EF Core
- Authentication and Authorization
- Order CRUD APIs
- Pagination and Bulk Import
- RabbitMQ messaging
- Docker Compose
- Unit and Integration Tests

## Getting Started

**Requirements:** .NET 10 SDK

Restore and build the solution:

```bash
dotnet restore OrderManagement.slnx
dotnet build OrderManagement.slnx
```

Run the API:

```bash
dotnet run --project src/OrderManagement.Api
```

API URL: http://localhost:5080

Health Check: http://localhost:5080/health/live

Run the Worker:

```bash
dotnet run --project src/OrderManagement.Worker
```

> The Worker currently runs without RabbitMQ integration.

## Project Structure

```text
src/
├── OrderManagement.Domain
├── OrderManagement.Application
├── OrderManagement.Infrastructure
├── OrderManagement.Api
└── OrderManagement.Worker
```

- **Domain:** Entities and business rules.
- **Application:** Business operations and interfaces.
- **Infrastructure:** Database and external services.
- **API:** HTTP endpoints.
- **Worker:** Background processing.

## Order Rules

- Each order must contain 1–100 unique products.
- Product prices must be positive.
- Order status follows: Pending → Confirmed → Shipped → Delivered.
- Invalid status changes are rejected.
- Deleted orders are soft-deleted.
- Inventory cannot have negative stock.
- Order changes are tracked with UTC timestamps and status history.

## Verification

Tested with **.NET SDK 10.0.401**.

- All 5 projects build successfully.
- 0 errors and 0 warnings.
- API health check returns `200 OK`.
- Worker starts successfully.
- 24 domain scenarios passed using a temporary test harness.

Formal automated tests will be added later.

## Development Progress

- Phase 1: Project Setup — Completed
- Phase 2: Domain Models & Rules — Completed
- Next: Database & Persistence

More details: `docs/phases-01-02.fa.md`
