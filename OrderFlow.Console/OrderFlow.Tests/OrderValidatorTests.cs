using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderValidatorTests
{
    // ── helper ────────────────────────────────────────────────────────────────
    private static Order MakeValidOrder() => new()
    {
        Id         = 1,
        CustomerId = 1,
        OrderDate  = DateTime.Now.AddDays(-1),
        Status     = OrderStatus.New,
        Items      =
        [
            new OrderItem
            {
                Id = 1, ProductId = 1, Quantity = 2, UnitPrice = 100m,
                Product = new Product { Id = 1, Name = "Test", Category = "X", Price = 100m }
            }
        ]
    };

    // ── Named rules (custom delegate) ─────────────────────────────────────────

    [Fact]
    public void ValidateAll_OrderWithoutItems_ReturnsError()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Items.Clear();

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public void ValidateAll_OrderExceedsAmountLimit_ReturnsError()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Items = [ new OrderItem { Quantity = 1, UnitPrice = 60_000m } ];

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("50000"));
    }

    [Fact]
    public void ValidateAll_ItemWithZeroQuantity_ReturnsError()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Items[0].Quantity = 0;

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
    }

    // ── Func<Order,bool> lambdy ───────────────────────────────────────────────

    [Fact]
    public void ValidateAll_OrderDateInFuture_ReturnsError()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.OrderDate = DateTime.Now.AddDays(5);

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateAll_CancelledOrder_ReturnsError()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Status = OrderStatus.Cancelled;

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateAll_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAll_OrderBreakingMultipleRules_ReturnsMultipleErrors()
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Items.Clear();
        order.Status    = OrderStatus.Cancelled;
        order.OrderDate = DateTime.Now.AddDays(3);

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.False(result.IsValid);
        Assert.True(result.Errors.Count >= 2);
    }

    // ── [Theory] — różne statusy ──────────────────────────────────────────────

    [Theory]
    [InlineData(OrderStatus.New,        true)]
    [InlineData(OrderStatus.Validated,  true)]
    [InlineData(OrderStatus.Processing, true)]
    [InlineData(OrderStatus.Completed,  true)]
    [InlineData(OrderStatus.Cancelled,  false)]
    [InlineData(OrderStatus.Failed,     true)]
    public void ValidateAll_VariousStatuses_ReturnsExpectedValidity(
        OrderStatus status, bool expectedValid)
    {
        // Arrange
        var validator = OrderValidator.CreateWithDefaults();
        var order = MakeValidOrder();
        order.Status = status;

        // Act
        var result = validator.ValidateAll(order);

        // Assert
        Assert.Equal(expectedValid, result.IsValid);
    }
}
