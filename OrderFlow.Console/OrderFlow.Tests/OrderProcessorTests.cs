using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Tests;

public class OrderProcessorTests
{
    // ── helpers ───────────────────────────────────────────────────────────────
    private static List<Order> MakeOrders()
    {
        var customer    = new Customer { Id = 1, FullName = "Jan",  IsVip = false };
        var vipCustomer = new Customer { Id = 2, FullName = "Anna", IsVip = true  };

        return
        [
            new Order { Id = 1, Status = OrderStatus.New,       Customer = customer,    CustomerId = 1,
                Items = [ new OrderItem { Quantity = 1, UnitPrice = 200m  } ] },
            new Order { Id = 2, Status = OrderStatus.Completed,  Customer = vipCustomer, CustomerId = 2,
                Items = [ new OrderItem { Quantity = 1, UnitPrice = 6000m } ] },
            new Order { Id = 3, Status = OrderStatus.Processing, Customer = customer,    CustomerId = 1,
                Items = [ new OrderItem { Quantity = 2, UnitPrice = 300m  } ] },
            new Order { Id = 4, Status = OrderStatus.New,        Customer = vipCustomer, CustomerId = 2,
                Items = [ new OrderItem { Quantity = 1, UnitPrice = 500m  } ] },
        ];
    }

    // ── FilterOrders (Predicate) ──────────────────────────────────────────────

    [Fact]
    public void FilterOrders_ByStatusNew_ReturnsOnlyNewOrders()
    {
        // Arrange
        var processor = new OrderProcessor();
        var orders    = MakeOrders();

        // Act
        var result = processor.FilterOrders(orders, OrderProcessorExtensions.ByStatus(OrderStatus.New));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.Equal(OrderStatus.New, o.Status));
    }

    [Fact]
    public void FilterOrders_FromVipCustomers_ReturnsOnlyVipOrders()
    {
        // Arrange
        var processor = new OrderProcessor();
        var orders    = MakeOrders();

        // Act
        var result = processor.FilterOrders(orders, OrderProcessorExtensions.FromVipCustomers());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, o => Assert.True(o.Customer.IsVip));
    }

    // ── Aggregate (Func) ──────────────────────────────────────────────────────

    [Fact]
    public void Aggregate_SumTotal_ReturnsSumOfAllOrders()
    {
        // Arrange
        var processor = new OrderProcessor();
        var orders    = MakeOrders();
        var expected  = orders.Sum(o => o.TotalAmount); // 200 + 6000 + 600 + 500 = 7300

        // Act
        var result = processor.Aggregate(orders, OrderProcessorExtensions.SumTotal());

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Aggregate_MaxTotal_ReturnsHighestOrderAmount()
    {
        // Arrange
        var processor = new OrderProcessor();
        var orders    = MakeOrders();

        // Act
        var result = processor.Aggregate(orders, OrderProcessorExtensions.MaxTotal());

        // Assert
        Assert.Equal(6000m, result);
    }

    // ── ProjectOrders (Func) ──────────────────────────────────────────────────

    [Fact]
    public void ProjectOrders_ToOrderInfo_ReturnsStringForEachOrder()
    {
        // Arrange
        var processor = new OrderProcessor();
        var orders    = MakeOrders();

        // Act
        var result = processor.ProjectOrders(orders, OrderProcessorExtensions.ProjectToOrderInfo());

        // Assert
        Assert.Equal(orders.Count, result.Count);
        Assert.All(result, s => Assert.False(string.IsNullOrWhiteSpace(s)));
    }
}
