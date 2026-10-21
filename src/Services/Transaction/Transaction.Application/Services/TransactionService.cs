using PersonalFinanceManager.Transactions.Domain;

namespace PersonalFinanceManager.Transactions.Application;

public record CreateTransactionRequest(Guid AccountId, string Type, decimal Amount, string Currency, string Description, Guid? DestinationAccountId = null);
public record TransactionResponse(Guid Id, Guid UserId, Guid AccountId, string Type, decimal Amount, string Currency, string Description, DateTime OccurredOnUtc);

public class TransactionService
{
    public Task<TransactionResponse> CreateAsync(CreateTransactionRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<TransactionType>(request.Type, true, out var transactionType))
        {
            throw new ArgumentException("Invalid transaction type.", nameof(request));
        }

        var entry = TransactionEntry.Create(
            userId,
            request.AccountId,
            transactionType,
            request.Amount,
            request.Currency,
            request.Description,
            request.DestinationAccountId);

        var response = new TransactionResponse(
            entry.Id,
            entry.UserId,
            entry.AccountId,
            entry.Type.ToString(),
            entry.Amount,
            entry.Currency,
            entry.Description,
            entry.OccurredOnUtc);

        return Task.FromResult(response);
    }
}
