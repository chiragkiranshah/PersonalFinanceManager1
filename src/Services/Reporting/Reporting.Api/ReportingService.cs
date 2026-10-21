namespace PersonalFinanceManager.Reporting;

public record FinancialSummary(decimal Income, decimal Expenses, decimal Savings, decimal NetCashFlow, decimal TotalBudget, decimal BudgetRemaining, decimal BudgetUtilizationPercent);

public interface IReportingRepository
{
    Task<decimal> GetTotalIncomeAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalExpensesAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
    Task<decimal> GetBudgetAmountAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);
}

public class ReportingService(IReportingRepository repository)
{
    public async Task<FinancialSummary> GetFinancialSummaryAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        if (fromUtc >= toUtc)
        {
            throw new ArgumentException("The start date must be earlier than the end date.");
        }

        var income = await repository.GetTotalIncomeAsync(userId, fromUtc, toUtc, cancellationToken);
        var expenses = await repository.GetTotalExpensesAsync(userId, fromUtc, toUtc, cancellationToken);
        var budget = await repository.GetBudgetAmountAsync(userId, fromUtc, toUtc, cancellationToken);

        var savings = income - expenses;
        var netCashFlow = income - expenses;
        var remaining = budget - expenses;
        var utilization = budget == 0m ? 0m : (expenses / budget) * 100m;

        return new FinancialSummary(income, expenses, savings, netCashFlow, budget, remaining, utilization);
    }
}
