using MassTransit;
using PersonalFinanceManager.Notifications.Domain;

namespace PersonalFinanceManager.Notifications.Api;

public sealed class BudgetThresholdExceededConsumer : IConsumer<BudgetThresholdExceededEvent>
{
    public Task Consume(ConsumeContext<BudgetThresholdExceededEvent> context)
    {
        var evt = context.Message;
        Console.WriteLine($"Notification for user {evt.UserId}: budget {evt.BudgetId} reached {evt.UtilizationPercent}%");
        return Task.CompletedTask;
    }
}
