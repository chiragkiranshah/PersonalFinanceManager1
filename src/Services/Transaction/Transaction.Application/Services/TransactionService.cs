using PersonalFinanceManager.Transactions.Domain;

namespace PersonalFinanceManager.Transactions.Application;

public record CreateTransactionRequest(Guid AccountId, string Type, decimal Amount, string Currency, string Description, string? Notes = null);
public record CreateTransferRequest(Guid SourceAccountId, Guid DestinationAccountId, decimal Amount, string Currency, string Description, string? Notes = null);
public record UpdateTransactionRequest(string Description, decimal Amount, string Currency, string? Notes = null);
public record TransactionResponse(Guid Id, Guid UserId, Guid AccountId, Guid? DestinationAccountId, string Type, decimal Amount, string Currency, string Description, string? Notes, DateTime OccurredOnUtc, DateTime UpdatedAtUtc);

public interface ITransactionRepository
{
    Task<TransactionEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TransactionEntry>> ListAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(TransactionEntry entry, CancellationToken cancellationToken = default);
    Task UpdateAsync(TransactionEntry entry, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

public class TransactionService(ITransactionRepository repository)
{
    public async Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var type))
        {
            throw new ArgumentException("Invalid transaction type.", nameof(request));
        }

        TransactionEntry entry = type switch
        {
            TransactionType.Income => TransactionEntry.CreateIncome(userId, request.AccountId, request.Amount, request.Currency, request.Description, request.Notes),
            TransactionType.Expense => TransactionEntry.CreateExpense(userId, request.AccountId, request.Amount, request.Currency, request.Description, request.Notes),
            _ => throw new InvalidOperationException("Use the transfer endpoint for transfers.")
        };

        await repository.AddAsync(entry, cancellationToken);
        return Map(entry);
    }

    public async Task<TransactionResponse> CreateTransferAsync(CreateTransferRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var entry = TransactionEntry.CreateTransfer(
            userId,
            request.SourceAccountId,
            request.DestinationAccountId,
            request.Amount,
            request.Currency,
            request.Description,
            request.Notes);

        await repository.AddAsync(entry, cancellationToken);
        return Map(entry);
    }

    public async Task<TransactionResponse?> GetAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null || !transaction.IsOwnedBy(userId))
        {
            return null;
        }

        return Map(transaction);
    }

    public async Task<IReadOnlyList<TransactionResponse>> ListAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var transactions = await repository.ListAsync(userId, cancellationToken);
        return transactions.Select(Map).ToList();
    }

    public async Task<TransactionResponse?> UpdateAsync(Guid id, UpdateTransactionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null || !transaction.IsOwnedBy(userId))
        {
            return null;
        }

        transaction.UpdateDetails(request.Description, request.Amount, request.Currency, request.Notes);
        await repository.UpdateAsync(transaction, cancellationToken);
        return Map(transaction);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var transaction = await repository.GetByIdAsync(id, cancellationToken);
        if (transaction is null || !transaction.IsOwnedBy(userId))
        {
            return false;
        }

        await repository.DeleteAsync(id, cancellationToken);
        return true;
    }

    private static TransactionResponse Map(TransactionEntry entry)
    {
        return new TransactionResponse(
            entry.Id,
            entry.UserId,
            entry.AccountId,
            entry.DestinationAccountId,
            entry.Type.ToString(),
            entry.Amount,
            entry.Currency,
            entry.Description,
            entry.Notes,
            entry.OccurredOnUtc,
            entry.UpdatedAtUtc);
    }
}
