using PersonalFinanceManager.Accounts.Domain;
using PersonalFinanceManager.Shared;

namespace Finance.Domain.Tests;

public class AccountTests
{
    [Fact]
    public void Create_WhenValidData_ShouldInitializeAccountState()
    {
        var userId = Guid.NewGuid();

        var account = Account.Create(userId, "Primary Checking", AccountType.Checking, "USD", 1200m);

        Assert.Equal(userId, account.UserId);
        Assert.Equal("Primary Checking", account.Name);
        Assert.Equal("USD", account.Currency);
        Assert.Equal(1200m, account.CurrentBalance);
        Assert.Equal(AccountStatus.Active, account.Status);
    }

    [Fact]
    public void Debit_WhenBalanceWouldGoNegative_ShouldThrow()
    {
        var account = Account.Create(Guid.NewGuid(), "Emergency", AccountType.Checking, "USD", 50m);

        Assert.Throws<InvalidOperationException>(() => account.ApplyDebit(51m));
    }

    [Fact]
    public void Credit_ShouldIncreaseCurrentBalance()
    {
        var account = Account.Create(Guid.NewGuid(), "Savings", AccountType.Savings, "USD", 100m);

        account.ApplyCredit(25m);

        Assert.Equal(125m, account.CurrentBalance);
    }

    [Fact]
    public void Money_WithDifferentCurrencies_ShouldThrow()
    {
        var left = new Money(100m, "USD");
        var right = new Money(50m, "EUR");

        Assert.Throws<InvalidOperationException>(() => left.Add(right));
    }

    [Fact]
    public void InvalidCurrency_ShouldThrow()
    {
        Assert.Throws<ArgumentException>(() => CurrencyCode.Validate("US"));
    }
}
