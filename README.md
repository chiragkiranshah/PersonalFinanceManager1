# Personal Finance Manager

This repository intentionally starts as a production-grade engineering foundation for a personal finance platform built around a clean microservice architecture.

## Phase 1: Architecture and service boundaries

### Proposed architecture

The platform is organized around the following business services:

- Identity Service: registration, login, token issuance, profile management, and identity data ownership.
- Account Service: financial accounts, ownership checks, balance management, and account history primitives.
- Transaction Service: income, expense, transfer, validation, and accounting effects.
- Category Service: category metadata and rules for safe deletion.
- Budget Service: budgets, thresholds, and utilization tracking.
- Reporting Service: financial summaries and read models.
- Notification Service: budget alerts and event-driven notifications.
- API Gateway: authentication, routing, rate limiting, and policy enforcement.

Each service owns its own database and persistence model. Cross-service access is done via HTTP APIs or integration events. The solution uses a shared kernel for reusable primitives such as currency handling, identity constants, and common serialization conventions.

### Key design choices

- Separate databases per service to preserve isolation and independent evolution.
- Decimal monetary values with explicit currency validation to avoid floating-point errors.
- Transactional outbox and idempotent consumers for financial events.
- Explicit transfer workflow to guarantee debits and credits are applied consistently.
- Resource-level authorization through user ownership checks.
- OpenTelemetry, Serilog, health checks, and consistent problem details for production readiness.

### Domain model summary

- UserProfile: identity and profile metadata.
- Account: user-owned financial account, account type, currency, opening balance, current balance.
- Transaction: income, expense, or transfer record with immutable accounting effects.
- Budget: category-based monthly/quarterly budget with thresholds.
- Category: user-defined or system-defined classification.
- NotificationPreference: delivery channel and thresholds.

### Database ownership strategy

The Account and Transaction services own their financial data. They do not directly read or modify each other's data. The reporting service maintains read models derived from published events. The identity service owns user and authentication metadata.

### Event-driven workflow

The system uses RabbitMQ and MassTransit for asynchronous events such as:

- AccountCreated
- AccountUpdated
- TransactionCreated
- TransactionDeleted
- BudgetThresholdExceeded
- NotificationRequested

The architecture enforces transactional outbox behavior so that database commits and event publication remain aligned.

## Phase 2 foundation is now being added

This repository contains the initial engineering foundation for the core finance flows:

- shared domain primitives
- account domain and application layers
- transaction domain and application layers
- API gateway initialization
- Docker Compose and environment template
- test project for domain invariants

The application is intentionally structured so additional microservices can be added without breaking the current architecture.

## Repository layout

```text
.
├── .env.example
├── Directory.Build.props
├── Directory.Packages.props
├── README.md
├── docker-compose.yml
├── docs/
│   ├── architecture-overview.md
│   └── service-boundaries.md
├── src/
│   ├── ApiGateway/
│   ├── BuildingBlocks/
│   │   └── Financial.Shared/
│   ├── Services/
│   │   ├── Account/
│   │   └── Transaction/
│   └── PersonalFinanceManager.sln (optional in later increments)
└── tests/
    └── Finance.Domain.Tests/
```

## Local run steps

1. Copy `.env.example` to `.env` and update connection strings and credentials.
2. Run `docker compose up --build`.
3. Browse the API docs at the gateway or service-level Swagger endpoints.
4. Use the account and transaction endpoints to exercise account creation and transaction processing.

### Example commands

```bash
docker compose up --build
docker compose down -v
```

### EF Core migrations

Generate and apply migrations in each service as the schema evolves:

```bash
dotnet ef migrations add InitialCreate --project src/Services/Account/Account.Infrastructure --startup-project src/Services/Account/Account.Api
dotnet ef database update --project src/Services/Account/Account.Infrastructure --startup-project src/Services/Account/Account.Api

# Repeat per service as needed.
```

## Current project scope

This repository implements:

- account domain rules and service abstractions
- transaction domain and service logic
- gateway reverse proxy setup
- shared money and currency validation primitives
- Docker-based infrastructure templates
- domain tests for core invariants

The remaining services (Identity, Budget, Reporting, Notification, and full event stream integration) can be added incrementally without altering the architectural patterns established here.

## Known limitations and assumptions

- This phase focuses on foundation and financial invariants rather than the full end-to-end distributed system.
- SQL Server and RabbitMQ dependencies are expected to be provided by Docker Compose.
- End-to-end event consumption and health monitoring remain to be expanded in subsequent phases.
- The API gateway is a reverse proxy skeleton and not yet connected to a central auth provider.

## Next phases

- Add Identity service and token issuance.
- Add Category and Budget services.
- Add Reporting service with read models.
- Add Notification service with MassTransit consumers.
- Add integration tests and e2e coverage.
- Add detailed OpenAPI security documentation and identity flows.

## Important note about build verification

The code in this repository is being checked in as a verifiable foundation, but the current environment does not provide a shell to run `dotnet build`. The repository should be validated with the standard .NET SDK and Docker tooling on a developer machine before claiming a full green build.
