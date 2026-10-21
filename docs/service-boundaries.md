# Service boundaries and persistence ownership

## Ownership rules

- Identity database: users, claims, auth, profile.
- Account database: accounts, balance snapshots, account lifecycle state.
- Transaction database: transactions, transfer ledger, event metadata.
- Category database: category definitions and category usage counters.
- Budget database: budget plans, thresholds, period definitions.
- Reporting database: materialized summaries and read projections.
- Notification database: notification preferences and delivery state.

## Cross-service interaction patterns

- Synchronous: user authentication, account lookup during transaction validation, read-only promotions.
- Asynchronous: notification delivery, reporting updates, budget threshold triggers.

## Security expectations

- every protected API uses JWT bearer tokens or OIDC integration
- resource-level checks enforce user ownership
- integration events omit secret values and all logs redact tokens
- all service DBs use least-privilege credentials
