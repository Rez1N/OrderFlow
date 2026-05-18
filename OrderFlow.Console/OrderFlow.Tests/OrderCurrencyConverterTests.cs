using Moq;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderCurrencyConverterTests
{
    private static Order MakeOrder(decimal totalAmount) => new()
    {
        Id    = 1,
        Items = [ new OrderItem { Quantity = 1, UnitPrice = totalAmount } ]
    };

    [Fact]
    public async Task ConvertOrderTotalAsync_ValidOrder_ReturnsConvertedAmount()
    {
        // Arrange
        var order = MakeOrder(1000m); // 1000 PLN
        var mockService = new Mock<ICurrencyService>();
        mockService
            .Setup(s => s.ConvertAsync(1000m, "PLN", "USD"))
            .ReturnsAsync(250m); // symulujemy 1000 PLN = 250 USD

        var converter = new OrderCurrencyConverter(mockService.Object);

        // Act
        var result = await converter.ConvertOrderTotalAsync(order, "USD");

        // Assert
        Assert.Equal(250m, result);
        mockService.Verify(s => s.ConvertAsync(1000m, "PLN", "USD"), Times.Once);
    }

    [Fact]
    public async Task ConvertOrderTotalAsync_ToEur_CallsServiceWithCorrectCurrency()
    {
        // Arrange
        var order = MakeOrder(500m);
        var mockService = new Mock<ICurrencyService>();
        mockService
            .Setup(s => s.ConvertAsync(500m, "PLN", "EUR"))
            .ReturnsAsync(119m);

        var converter = new OrderCurrencyConverter(mockService.Object);

        // Act
        var result = await converter.ConvertOrderTotalAsync(order, "EUR");

        // Assert
        Assert.Equal(119m, result);
        mockService.Verify(s => s.ConvertAsync(It.IsAny<decimal>(), "PLN", "EUR"), Times.Once);
    }
}
