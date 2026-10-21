namespace PersonalFinanceManager.Transactions.Domain;

public enum TransactionType
{
    Income,
    Expense,
    Transfer
}

public class TransactionEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid AccountId { get; private set; }
    public Guid? DestinationAccountId { get; private set; }
    public TransactionType Type { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public string Description { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }

    private TransactionEntry()
    {
        Currency = string.Empty;
        Description = string.Empty;
    }

    public static TransactionEntry Create(Guid userId, Guid accountId, TransactionType type, decimal amount, string currency, string description, Guid? destinationAccountId = null)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (accountId == Guid.Empty)
        {
            throw new ArgumentException("Account identifier is required.", nameof(accountId));
        }

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        return new TransactionEntry
        {
            UserId = userId,
            AccountId = accountId,
            Type = type,
            Amount = amount,
            Currency = currency.Trim().ToUpperInvariant(),
            Description = description.Trim(),
            DestinationAccountId = destinationAccountId,
            OccurredOnUtc = DateTime.UtcNow
        };
    }
}
