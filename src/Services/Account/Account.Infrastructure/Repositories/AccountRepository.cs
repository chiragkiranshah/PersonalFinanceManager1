using Microsoft.EntityFrameworkCore;
using PersonalFinanceManager.Accounts.Application;
using PersonalFinanceManager.Accounts.Domain;

namespace PersonalFinanceManager.Accounts.Infrastructure;

public class AccountRepository(AccountDbContext context) : IAccountRepository
{
    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Accounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.Accounts
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        await context.Accounts.AddAsync(account, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Account account, CancellationToken cancellationToken = default)
    {
        context.Accounts.Update(account);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (account is null)
        {
            return;
        }

        context.Accounts.Remove(account);
        await context.SaveChangesAsync(cancellationToken);
    }
}
