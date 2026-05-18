using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace OrderFlow.Console.Services;

public class CurrencyService : ICurrencyService
{
    private readonly HttpClient _client;

    // Bonus: prosty cache — kurs pobrany raz nie jest pobierany ponownie
    private readonly Dictionary<string, decimal> _cache = new(StringComparer.OrdinalIgnoreCase);

    public CurrencyService(HttpClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Zwraca kurs waluty względem PLN.
    /// Dla PLN zwraca 1 bez wywołania API.
    /// Dla nieistniejącej waluty (404) zwraca null.
    /// Dla innych błędów rzuca CurrencyServiceException.
    /// </summary>
    public async Task<decimal?> GetRateAsync(string currencyCode)
    {
        if (string.Equals(currencyCode, "PLN", StringComparison.OrdinalIgnoreCase))
            return 1.0m;

        // Sprawdź cache
        if (_cache.TryGetValue(currencyCode, out var cached))
            return cached;

        var url = $"https://api.nbp.pl/api/exchangerates/rates/A/{currencyCode.ToUpper()}/?format=json";

        try
        {
            var response = await _client.GetFromJsonAsync<NbpRateResponse>(url);
            var rate = response?.Rates?.FirstOrDefault()?.Mid;

            if (rate.HasValue)
                _cache[currencyCode] = rate.Value;

            return rate;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (HttpRequestException ex)
        {
            throw new CurrencyServiceException($"Błąd pobierania kursu dla {currencyCode}: {ex.Message}", ex);
        }
        catch (TaskCanceledException)
        {
            throw new CurrencyServiceException($"Timeout przy pobieraniu kursu dla {currencyCode}");
        }
    }

    /// <summary>
    /// Przelicza kwotę z fromCurrency na toCurrency przez PLN jako wspólny mianownik.
    /// </summary>
    public async Task<decimal> ConvertAsync(decimal amount, string fromCurrency, string toCurrency)
    {
        var fromRate = await GetRateAsync(fromCurrency)
            ?? throw new CurrencyServiceException($"Nieznana waluta: {fromCurrency}");

        var toRate = await GetRateAsync(toCurrency)
            ?? throw new CurrencyServiceException($"Nieznana waluta: {toCurrency}");

        // amount w fromCurrency → PLN → toCurrency
        var inPln = amount * fromRate;
        return inPln / toRate;
    }
}

// ── Modele do deserializacji odpowiedzi NBP ───────────────────────────────────

public class NbpRateResponse
{
    [JsonPropertyName("table")]    public string Table    { get; set; } = "";
    [JsonPropertyName("currency")] public string Currency { get; set; } = "";
    [JsonPropertyName("code")]     public string Code     { get; set; } = "";
    [JsonPropertyName("rates")]    public List<NbpRate> Rates { get; set; } = new();
}

public class NbpRate
{
    [JsonPropertyName("no")]            public string  No            { get; set; } = "";
    [JsonPropertyName("effectiveDate")] public string  EffectiveDate { get; set; } = "";
    [JsonPropertyName("mid")]           public decimal Mid           { get; set; }
}
