using System.Text.RegularExpressions;

namespace PersonalFinanceManager.Shared;

public static partial class CurrencyCode
{
    public static string Validate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Currency code is required.", nameof(value));
        }

        var normalized = value.Trim();
        if (!IsValidCurrencyCode().IsMatch(normalized))
        {
            throw new ArgumentException("Currency code must be a valid ISO 4217 code with three letters.", nameof(value));
        }

        return normalized.ToUpperInvariant();
    }

    [GeneratedRegex("^[A-Z]{3}$", RegexOptions.CultureInvariant)]
    private static partial Regex IsValidCurrencyCode();
}

public readonly record struct Money(decimal Amount, string CurrencyCode)
{
    public static Money Zero(string currencyCode) => new(0m, CurrencyCode.Validate(currencyCode));

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(Amount + other.Amount, CurrencyCode);
    }

    public Money Subtract(Money other)
    {
        EnsureSameCurrency(other);

        return new Money(Amount - other.Amount, CurrencyCode);
    }

    public bool IsZero => Amount == 0m;

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(CurrencyCode, other.CurrencyCode, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Money operations require the same currency code.");
        }
    }
}
