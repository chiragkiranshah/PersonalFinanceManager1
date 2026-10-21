namespace PersonalFinanceManager.Budgets.Domain;

public enum BudgetPeriod
{
    Monthly,
    Quarterly,
    Yearly
}

public class Budget
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public Guid CategoryId { get; private set; }
    public string Name { get; private set; }
    public BudgetPeriod Period { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public decimal SpentAmount { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime StartDateUtc { get; private set; }
    public DateTime EndDateUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Budget()
    {
        Name = string.Empty;
        Currency = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Budget Create(Guid userId, Guid categoryId, string name, BudgetPeriod period, decimal amount, string currency, DateTime startDateUtc, DateTime endDateUtc, bool isActive = true)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier is required.", nameof(userId));
        if (categoryId == Guid.Empty) throw new ArgumentException("Category identifier is required.", nameof(categoryId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Budget name is required.", nameof(name));
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Budget amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        if (startDateUtc >= endDateUtc) throw new ArgumentException("Budget end date must be after the start date.");

        return new Budget
        {
            UserId = userId,
            CategoryId = categoryId,
            Name = name.Trim(),
            Period = period,
            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero),
            Currency = currency.Trim().ToUpperInvariant(),
            SpentAmount = 0m,
            IsActive = isActive,
            StartDateUtc = startDateUtc,
            EndDateUtc = endDateUtc,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, BudgetPeriod period, decimal amount, string currency, DateTime startDateUtc, DateTime endDateUtc, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Budget name is required.", nameof(name));
        if (amount <= 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Budget amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(currency)) throw new ArgumentException("Currency is required.", nameof(currency));
        if (startDateUtc >= endDateUtc) throw new ArgumentException("Budget end date must be after the start date.");

        Name = name.Trim();
        Period = period;
        Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        Currency = currency.Trim().ToUpperInvariant();
        StartDateUtc = startDateUtc;
        EndDateUtc = endDateUtc;
        IsActive = isActive;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddExpense(decimal amount)
    {
        if (amount < 0m) throw new ArgumentOutOfRangeException(nameof(amount), "Expense amount cannot be negative.");
        SpentAmount = decimal.Round(SpentAmount + amount, 2, MidpointRounding.AwayFromZero);
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public decimal GetRemaining() => Amount - SpentAmount;
    public decimal GetUtilizationPercent() => Amount == 0m ? 0m : (SpentAmount / Amount) * 100m;
    public bool IsOwnedBy(Guid userId) => UserId == userId;
}
