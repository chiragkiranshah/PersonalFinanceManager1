using PersonalFinanceManager.Categories.Domain;
using PersonalFinanceManager.Budgets.Domain;
using PersonalFinanceManager.Reporting;

namespace Finance.Domain.Tests;

public class CategoryBudgetAndReportingTests
{
    [Fact]
    public void CreateCategory_ShouldSetDomainState()
    {
        var userId = Guid.NewGuid();
        var category = PersonalFinanceManager.Categories.Domain.Category.Create(userId, "Food", CategoryType.Expense);

        Assert.Equal(userId, category.UserId);
        Assert.Equal("Food", category.Name);
        Assert.Equal(CategoryType.Expense, category.Type);
    }

    [Fact]
    public void CreateBudget_ShouldCalculateUtilization()
    {
        var budget = Budget.Create(Guid.NewGuid(), Guid.NewGuid(), "Groceries", BudgetPeriod.Monthly, 1000m, "USD", DateTime.UtcNow, DateTime.UtcNow.AddDays(30));
        budget.AddExpense(400m);

        Assert.Equal(600m, budget.GetRemaining());
        Assert.Equal(40m, budget.GetUtilizationPercent());
    }

    [Fact]
    public void ReportingService_ShouldSummarizeFinancials()
    {
        var repository = new StubReportingRepository();
        var service = new ReportingService(repository);

        var summary = service.GetFinancialSummaryAsync(Guid.NewGuid(), DateTime.UtcNow.AddDays(-30), DateTime.UtcNow).GetAwaiter().GetResult();

        Assert.Equal(12500m, summary.Income);
        Assert.Equal(8600m, summary.Expenses);
        Assert.Equal(3900m, summary.Savings);
    }

    private sealed class StubReportingRepository : IReportingRepository
    {
        public Task<decimal> GetTotalIncomeAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(12500m);
        public Task<decimal> GetTotalExpensesAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(8600m);
        public Task<decimal> GetBudgetAmountAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default) => Task.FromResult(11000m);
    }
}
