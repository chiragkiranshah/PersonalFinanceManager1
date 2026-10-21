using PersonalFinanceManager.Shared;

namespace PersonalFinanceManager.Accounts.Domain;

public enum AccountType
{
    Checking,
    Savings,
    Cash,
    CreditCard
}

public enum AccountStatus
{
    Active,
    Closed
}

public class Account
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public AccountType Type { get; private set; }
    public string Currency { get; private set; }
    public decimal OpeningBalance { get; private set; }
    public decimal CurrentBalance { get; private set; }
    public AccountStatus Status { get; private set; } = AccountStatus.Active;
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; private set; } = DateTime.UtcNow;

    private Account()
    {
        Name = string.Empty;
        Currency = string.Empty;
    }

    public static Account Create(Guid userId, string name, AccountType type, string currency, decimal openingBalance)
    {
        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User identifier is required.", nameof(userId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Account name is required.", nameof(name));
        }

        var normalizedCurrency = CurrencyCode.Validate(currency);

        if (openingBalance < 0m && type != AccountType.CreditCard)
        {
            throw new InvalidOperationException("Opening balances for non-credit accounts must be zero or positive.");
        }

        return new Account
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            Currency = normalizedCurrency,
            OpeningBalance = openingBalance,
            CurrentBalance = openingBalance,
            Status = AccountStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, AccountType type, string currency)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Account name is required.", nameof(name));
        }

        Name = name.Trim();
        Type = type;
        Currency = CurrencyCode.Validate(currency);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyCredit(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be greater than zero.");
        }

        CurrentBalance += amount;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ApplyDebit(decimal amount)
    {
        if (amount <= 0m)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be greater than zero.");
        }

        var projectedBalance = CurrentBalance - amount;
        if (Type != AccountType.CreditCard && projectedBalance < 0m)
        {
            throw new InvalidOperationException("Insufficient funds for this account.");
        }

        CurrentBalance = projectedBalance;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Close()
    {
        if (Status == AccountStatus.Closed)
        {
            throw new InvalidOperationException("Account is already closed.");
        }

        Status = AccountStatus.Closed;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
