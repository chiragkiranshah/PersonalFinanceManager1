using PersonalFinanceManager.Budgets.Domain;

namespace PersonalFinanceManager.Budgets.Application;

public record CreateBudgetRequest(Guid CategoryId, string Name, string Period, decimal Amount, string Currency, DateTime StartDateUtc, DateTime EndDateUtc, bool IsActive = true);
public record UpdateBudgetRequest(string Name, string Period, decimal Amount, string Currency, DateTime StartDateUtc, DateTime EndDateUtc, bool IsActive = true);
public record BudgetResponse(Guid Id, Guid UserId, Guid CategoryId, string Name, string Period, decimal Amount, string Currency, decimal SpentAmount, decimal RemainingAmount, decimal UtilizationPercent, bool IsActive, DateTime StartDateUtc, DateTime EndDateUtc, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Budget>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Budget budget, CancellationToken cancellationToken = default);
    Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class BudgetService(IBudgetRepository repository)
{
    public async Task<BudgetResponse> CreateAsync(CreateBudgetRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<BudgetPeriod>(request.Period, true, out var period))
        {
            throw new ArgumentException("Invalid budget period.", nameof(request));
        }

        var budget = Budget.Create(userId, request.CategoryId, request.Name, period, request.Amount, request.Currency, request.StartDateUtc, request.EndDateUtc, request.IsActive);
        await repository.AddAsync(budget, cancellationToken);
        return Map(budget);
    }

    public async Task<BudgetResponse?> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var budget = await repository.GetByIdAsync(id, cancellationToken);
        if (budget is null || !budget.IsOwnedBy(userId)) return null;
        return Map(budget);
    }

    public async Task<IReadOnlyList<BudgetResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var budgets = await repository.ListAsync(userId, cancellationToken);
        return budgets.Select(Map).ToList();
    }

    public async Task<BudgetResponse?> UpdateAsync(Guid id, UpdateBudgetRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var budget = await repository.GetByIdAsync(id, cancellationToken);
        if (budget is null || !budget.IsOwnedBy(userId)) return null;

        if (!Enum.TryParse<BudgetPeriod>(request.Period, true, out var period))
        {
            throw new ArgumentException("Invalid budget period.", nameof(request));
        }

        budget.Update(request.Name, period, request.Amount, request.Currency, request.StartDateUtc, request.EndDateUtc, request.IsActive);
        await repository.UpdateAsync(budget, cancellationToken);
        return Map(budget);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var budget = await repository.GetByIdAsync(id, cancellationToken);
        if (budget is null || !budget.IsOwnedBy(userId)) return false;

        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static BudgetResponse Map(Budget budget) => new(
        budget.Id,
        budget.UserId,
        budget.CategoryId,
        budget.Name,
        budget.Period.ToString(),
        budget.Amount,
        budget.Currency,
        budget.SpentAmount,
        budget.GetRemaining(),
        budget.GetUtilizationPercent(),
        budget.IsActive,
        budget.StartDateUtc,
        budget.EndDateUtc,
        budget.CreatedAtUtc,
        budget.UpdatedAtUtc);
}
