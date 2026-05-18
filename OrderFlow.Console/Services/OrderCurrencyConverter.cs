using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Przelicza wartość zamówień na inne waluty używając ICurrencyService.
/// </summary>
public class OrderCurrencyConverter
{
    private readonly ICurrencyService _currencyService;

    public OrderCurrencyConverter(ICurrencyService currencyService)
    {
        _currencyService = currencyService;
    }

    /// <summary>
    /// Zwraca TotalAmount zamówienia przeliczone na podaną walutę.
    /// </summary>
    public async Task<decimal> ConvertOrderTotalAsync(Order order, string targetCurrency)
    {
        return await _currencyService.ConvertAsync(order.TotalAmount, "PLN", targetCurrency);
    }
}
