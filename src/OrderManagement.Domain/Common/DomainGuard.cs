namespace OrderManagement.Domain.Common;

internal static class DomainGuard
{
    private const decimal MaximumMoney = 9_999_999_999_999_999.99m;

    internal static string RequiredText(string? value, int maximumLength, string field)
    {
        string trimmed = value?.Trim() ?? string.Empty;

        if (trimmed.Length == 0 || trimmed.Length > maximumLength)
        {
            throw new DomainException("validation.invalid_text",
                $"{field} must contain between 1 and {maximumLength} characters.");
        }

        return trimmed;
    }

    internal static void PositiveId(long value, string field)
    {
        if (value <= 0)
        {
            throw new DomainException("validation.invalid_id", $"{field} must be positive.");
        }
    }

    internal static void PositiveQuantity(int value)
    {
        if (value <= 0)
        {
            throw new DomainException("validation.invalid_quantity", "Quantity must be positive.");
        }
    }

    internal static void PositiveMoney(decimal value, string field)
    {
        if (value <= 0 || value > MaximumMoney || decimal.Round(value, 2) != value)
        {
            throw new DomainException("validation.invalid_money",
                $"{field} must be positive, fit decimal(18, 2), and have at most two decimal places.");
        }
    }
}
