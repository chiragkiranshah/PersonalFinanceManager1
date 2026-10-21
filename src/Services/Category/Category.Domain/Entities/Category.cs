namespace PersonalFinanceManager.Categories.Domain;

public enum CategoryType
{
    Income,
    Expense
}

public class Category
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Name { get; private set; }
    public CategoryType Type { get; private set; }
    public bool IsSystemDefined { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private Category()
    {
        Name = string.Empty;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public static Category Create(Guid userId, string name, CategoryType type, bool isSystemDefined = false)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.", nameof(name));

        return new Category
        {
            UserId = userId,
            Name = name.Trim(),
            Type = type,
            IsSystemDefined = isSystemDefined,
            IsDeleted = false,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }

    public void Update(string name, CategoryType type)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Category name is required.", nameof(name));
        Name = name.Trim();
        Type = type;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (IsSystemDefined)
        {
            throw new InvalidOperationException("System-defined categories cannot be deleted.");
        }

        IsDeleted = true;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public bool IsOwnedBy(Guid userId) => UserId == userId;
}
