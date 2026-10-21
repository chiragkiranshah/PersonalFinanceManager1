using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Transactions.Application;
using PersonalFinanceManager.Transactions.Domain;

namespace PersonalFinanceManager.Transactions.Infrastructure;

public class TransactionRepository(TransactionDbContext context) : ITransactionRepository
{
    public async Task<TransactionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Transactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<TransactionEntry>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.Transactions
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OccurredOnUtc)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TransactionEntry entry, CancellationToken cancellationToken = default)
    {
        await context.Transactions.AddAsync(entry, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TransactionEntry entry, CancellationToken cancellationToken = default)
    {
        context.Transactions.Update(entry);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var transaction = await context.Transactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (transaction is null)
        {
            return;
        }

        context.Transactions.Remove(transaction);
        await context.SaveChangesAsync(cancellationToken);
    }
}
