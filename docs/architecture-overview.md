# Architecture overview

This document records the design decisions that shape the Personal Finance Manager solution.

## Service boundaries

1. Identity Service
   - owns users, roles, profile, credentials, and token attributes.
   - exposes endpoints for registration, login, refresh token, and profile retrieval.

2. Account Service
   - owns account entities and current balances.
   - validates account lifecycle and user ownership.
   - emits account-related integration events.

3. Transaction Service
   - owns transaction records and financial effects.
   - ensures transfer workflows treat debits and credits as a single business action.
   - applies balance updates within the transaction service and publishes completion events.

4. Category Service
   - owns categories and safe deletion validation logic.

5. Budget Service
   - owns budget definitions and spending thresholds.
   - may publish threshold events to the notification service.

6. Reporting Service
   - maintains reporting read models and summary queries.
   - subscribes to account and transaction integration events.

7. Notification Service
   - processes budget alerts and user notifications.
   - uses RabbitMQ-backed consumers with retry and dead-letter handling.

8. API Gateway
   - exposes a single external entry point.
   - enforces auth, rate limiting, and request transformations.

## Shared kernel

The `Financial.Shared` library contains reusable value objects and rules for monetary operations, including explicit currency validation. This prevents each service from inventing incompatible money logic.

## Transaction invariants

The most critical invariant is that the sum of all ledger effects must preserve the account balance. Transactions must not be created without a valid account owner and currency. Deletions and updates must adjust balances consistently; the business logic should never rely on a cross-service database transaction.

## Event-driven flow

1. A command changes local state in a service database.
2. The same transaction writes an outbox record.
3. An outbox dispatcher publishes the event to RabbitMQ.
4. Consumers process the event idempotently and update derived read models.

This approach keeps local transactions reliable while allowing eventual consistency.
