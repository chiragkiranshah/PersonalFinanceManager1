using PersonalFinanceManager.Accounts.Domain;

namespace PersonalFinanceManager.Accounts.Application;

public record CreateAccountRequest(string Name, string AccountType, string Currency, decimal OpeningBalance);
public record AccountResponse(Guid Id, Guid UserId, string Name, string AccountType, string Currency, decimal OpeningBalance, decimal CurrentBalance, string Status);

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Account>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task UpdateAsync(Account account, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class AccountService(IAccountRepository repository)
{
    public async Task<AccountResponse> CreateAsync(CreateAccountRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<AccountType>(request.AccountType, true, out var accountType))
        {
            throw new ArgumentException("Invalid account type.", nameof(request));
        }

        var account = Account.Create(userId, request.Name, accountType, request.Currency, request.OpeningBalance);
        await repository.AddAsync(account, cancellationToken);

        return Map(account);
    }

    public async Task<AccountResponse?> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var account = await repository.GetByIdAsync(id, cancellationToken);
        if (account is null || account.UserId != userId)
        {
            return null;
        }

        return Map(account);
    }

    public async Task<IReadOnlyList<AccountResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var accounts = await repository.ListAsync(userId, cancellationToken);
        return accounts.Select(Map).ToList();
    }

    private static AccountResponse Map(Account account) => new(
        account.Id,
        account.UserId,
        account.Name,
        account.Type.ToString(),
        account.Currency,
        account.OpeningBalance,
        account.CurrentBalance,
        account.Status.ToString());
}
