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
    public string? Notes { get; private set; }
    public DateTime OccurredOnUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private TransactionEntry()
    {
        Currency = string.Empty;
        Description = string.Empty;
        OccurredOnUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static TransactionEntry CreateIncome(Guid userId, Guid accountId, decimal amount, string currency, string description, string? notes = null)
    {
        return Create(userId, accountId, TransactionType.Income, amount, currency, description, null, notes);
    }

    public static TransactionEntry CreateExpense(Guid userId, Guid accountId, decimal amount, string currency, string description, string? notes = null)
    {
        return Create(userId, accountId, TransactionType.Expense, amount, currency, description, null, notes);
    }

    public static TransactionEntry CreateTransfer(Guid userId, Guid sourceAccountId, Guid destinationAccountId, decimal amount, string currency, string description, string? notes = null)
    {
        if (sourceAccountId == Guid.Empty)
        {
            throw new ArgumentException("Source account is required.", nameof(sourceAccountId));
        }

        if (destinationAccountId == Guid.Empty)
        {
            throw new ArgumentException("Destination account is required.", nameof(destinationAccountId));
        }

        if (sourceAccountId == destinationAccountId)
        {
            throw new InvalidOperationException("A transfer must use two different accounts.");
        }

        return Create(userId, sourceAccountId, TransactionType.Transfer, amount, currency, description, destinationAccountId, notes);
    }

    public void UpdateDetails(string description, decimal amount, string currency, string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Transaction amount must be greater than zero.");
        }

        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = NormalizeCurrency(currency);
        Description = description.Trim();
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsOwnedBy(Guid userId)
    {
        return UserId == userId;
    }

    private static TransactionEntry Create(Guid userId, Guid accountId, TransactionType type, decimal amount, string currency, string description, Guid? destinationAccountId, string? notes)
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

        var normalizedCurrency = NormalizeCurrency(currency);

        return new TransactionEntry
        {
            UserId = userId,
            AccountId = accountId,
            DestinationAccountId = destinationAccountId,
            Type = type,
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            Currency = normalizedCurrency,
            Description = description.Trim(),
            Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim(),
            OccurredOnUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    private static string NormalizeCurrency(string currency)
    {
        if (string.IsNullOrWhiteSpace(currency))
        {
            throw new ArgumentException("Currency code is required.", nameof(currency));
        }

        var value = currency.Trim();
        if (value.Length != 3 || !value.All(char.IsLetter))
        {
            throw new ArgumentException("Currency code must be a three-letter ISO code.", nameof(currency));
        }

        return value.ToUpperInvariant();
    }
}
