using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Budgets.Application;
using PersonalFinanceManager.Budgets.Domain;

namespace PersonalFinanceManager.Budgets.Infrastructure;

public class BudgetRepository(BudgetDbContext context) : IBudgetRepository
{
    public async Task<Budget?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Budgets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Budget>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.Budgets
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.StartDateUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        await context.Budgets.AddAsync(budget, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Budget budget, CancellationToken cancellationToken = default)
    {
        context.Budgets.Update(budget);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var budget = await context.Budgets.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (budget is null) return;
        context.Budgets.Remove(budget);
        await context.SaveChangesAsync(cancellationToken);
    }
}
