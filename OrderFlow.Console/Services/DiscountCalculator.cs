using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Serwis obliczający rabaty dla zamówień.
/// Zbudowany metodą TDD (Red-Green-Refactor).
/// </summary>
public class DiscountCalculator
{
    // ── Stałe (Refactor: wyciągnięte ze stałych literałów) ───────────────────
    private const decimal VipDiscountRate        = 0.10m;   // 10%
    private const decimal HighValueDiscountRate  = 0.05m;   // 5%
    private const decimal VipHighValueExtraRate  = 0.05m;   // dodatkowe 5% dla VIP > 5000
    private const decimal MaxDiscountRate        = 0.25m;   // cap 25%
    private const decimal HighValueThreshold     = 1000m;
    private const decimal VipHighValueThreshold  = 5000m;

    /// <summary>
    /// Oblicza rabat jako kwotę w PLN.
    /// </summary>
    public decimal Calculate(Order order, Customer customer)
    {
        var rate = CalculateRate(order, customer);
        var capped = Math.Min(rate, MaxDiscountRate);
        return order.TotalAmount * capped;
    }

    // ── Metody pomocnicze (Refactor) ──────────────────────────────────────────

    private static decimal CalculateRate(Order order, Customer customer)
    {
        decimal rate = 0m;

        if (customer.IsVip)
            rate += VipDiscountRate;

        if (order.TotalAmount > HighValueThreshold)
            rate += HighValueDiscountRate;

        if (customer.IsVip && order.TotalAmount > VipHighValueThreshold)
            rate += VipHighValueExtraRate;

        return rate;
    }
}
