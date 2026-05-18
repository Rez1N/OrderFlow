namespace OrderFlow.Console.Services;

public interface ICurrencyService
{
    Task<decimal?> GetRateAsync(string currencyCode);
    Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency);
}

public class CurrencyServiceException : Exception
{
    public CurrencyServiceException(string message) : base(message) { }
    public CurrencyServiceException(string message, Exception inner) : base(message, inner) { }
}
