using System.Net;
using System.Text;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

// ── Helper: TestHttpMessageHandler ───────────────────────────────────────────

public class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;
    public int CallCount { get; private set; }

    public TestHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        _responder = responder;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        CallCount++;
        return Task.FromResult(_responder(request));
    }
}

// ── Testy CurrencyService ─────────────────────────────────────────────────────

public class CurrencyServiceTests
{
    private static string UsdJson(decimal mid = 3.9512m) => $$"""
        {
          "table": "A",
          "currency": "dolar amerykanski",
          "code": "USD",
          "rates": [
            { "no": "086/A/NBP/2026", "effectiveDate": "2026-05-06", "mid": {{mid}} }
          ]
        }
        """;

    private static CurrencyService MakeService(HttpMessageHandler handler)
        => new(new HttpClient(handler) { BaseAddress = null });

    // ── Test 1: Happy path ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRateAsync_ValidCurrency_ReturnsRate()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(UsdJson(3.9512m), Encoding.UTF8, "application/json")
            });
        var service = MakeService(handler);

        // Act
        var rate = await service.GetRateAsync("USD");

        // Assert
        Assert.Equal(3.9512m, rate);
    }

    // ── Test 2: PLN → zwraca 1 bez wywołania API ─────────────────────────────

    [Fact]
    public async Task GetRateAsync_PLN_Returns1WithoutCallingApi()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ =>
            throw new Exception("API nie powinno byc wywolane dla PLN!"));
        var service = MakeService(handler);

        // Act
        var rate = await service.GetRateAsync("PLN");

        // Assert
        Assert.Equal(1.0m, rate);
        Assert.Equal(0, handler.CallCount);
    }

    // ── Test 3: 404 → null ────────────────────────────────────────────────────

    [Fact]
    public async Task GetRateAsync_UnknownCurrency404_ReturnsNull()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent("404 NotFound")
            });
        var service = MakeService(handler);

        // Act
        var rate = await service.GetRateAsync("XYZ");

        // Assert
        Assert.Null(rate);
    }

    // ── Test 4: 500 → CurrencyServiceException ────────────────────────────────

    [Fact]
    public async Task GetRateAsync_ServerError500_ThrowsCurrencyServiceException()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
        var service = MakeService(handler);

        // Act & Assert
        await Assert.ThrowsAsync<CurrencyServiceException>(
            () => service.GetRateAsync("USD"));
    }

    // ── Test 5: ConvertAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task ConvertAsync_UsdToEur_ReturnsCorrectAmount()
    {
        // Arrange — USD = 4.0 PLN, EUR = 4.2 PLN
        // 100 USD → 400 PLN → 400/4.2 ≈ 95.24 EUR
        var handler = new TestHttpMessageHandler(request =>
        {
            var url = request.RequestUri!.ToString();
            var mid = url.Contains("USD") ? 4.0m : 4.2m; // USD lub EUR
            var code = url.Contains("USD") ? "USD" : "EUR";
            var json = $$"""
                {"table":"A","currency":"test","code":"{{code}}",
                 "rates":[{"no":"001","effectiveDate":"2026-01-01","mid":{{mid}}}]}
                """;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
        });
        var service = MakeService(handler);

        // Act
        var result = await service.ConvertAsync(100m, "USD", "EUR");

        // Assert — 100 * 4.0 / 4.2 ≈ 95.238...
        Assert.Equal(100m * 4.0m / 4.2m, result);
    }

    // ── Test 6: weryfikacja URL ───────────────────────────────────────────────

    [Fact]
    public async Task GetRateAsync_BuildsCorrectUrl()
    {
        // Arrange
        HttpRequestMessage? capturedRequest = null;
        var handler = new TestHttpMessageHandler(request =>
        {
            capturedRequest = request;
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(UsdJson(), Encoding.UTF8, "application/json")
            };
        });
        var service = MakeService(handler);

        // Act
        await service.GetRateAsync("EUR");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Contains("EUR", capturedRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Get, capturedRequest.Method);
    }

    // ── Bonus Test 7: cache — API wywołane tylko raz ─────────────────────────

    [Fact]
    public async Task GetRateAsync_CalledTwiceForSameCurrency_ApiCalledOnlyOnce()
    {
        // Arrange
        var handler = new TestHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(UsdJson(4.0m), Encoding.UTF8, "application/json")
            });
        var service = MakeService(handler);

        // Act
        var rate1 = await service.GetRateAsync("USD");
        var rate2 = await service.GetRateAsync("USD");

        // Assert
        Assert.Equal(rate1, rate2);
        Assert.Equal(1, handler.CallCount); // API trafione tylko raz
    }
}
