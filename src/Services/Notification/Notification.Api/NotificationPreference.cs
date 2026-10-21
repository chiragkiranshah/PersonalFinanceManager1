namespace PersonalFinanceManager.Notifications.Domain;

public record BudgetThresholdExceededEvent(Guid UserId, Guid BudgetId, Guid CategoryId, decimal BudgetAmount, decimal SpentAmount, decimal UtilizationPercent, DateTime OccurredAtUtc);

public class NotificationPreference
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public string Email { get; private set; }
    public bool EmailEnabled { get; private set; }
    public bool PushEnabled { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    private NotificationPreference() { Email = string.Empty; CreatedAtUtc = DateTime.UtcNow; UpdatedAtUtc = DateTime.UtcNow; }

    public static NotificationPreference Create(Guid userId, string email, bool emailEnabled = true, bool pushEnabled = false)
    {
        if (userId == Guid.Empty) throw new ArgumentException("User identifier is required.", nameof(userId));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));

        return new NotificationPreference
        {
            UserId = userId,
            Email = email.Trim(),
            EmailEnabled = emailEnabled,
            PushEnabled = pushEnabled,
            CreatedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };
    }
}
