using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Console.Lab2;

/// <summary>Lab 2 — zadanie 3: thread safety w statystykach.</summary>
public static class Lab2Task3Statistics
{
    public static void Run()
    {
        System.Console.WriteLine("\n" + new string('=', 70));
        System.Console.WriteLine("LAB 2 — ZADANIE 3: Thread safety (statystyki)".PadRight(70));
        System.Console.WriteLine(new string('=', 70));

        var products = SampleData.GetProducts();
        var customers = SampleData.GetCustomers();
        var template = BuildTemplateOrders(products, customers);
        var statsOrders = Enumerable.Range(0, 80).SelectMany(_ => CloneOrders(template)).ToList();

        OrderStatisticsDemos.RunUnsafeParallelDemo(statsOrders, runs: 12);
        OrderStatisticsDemos.RunSafeParallelDemo(statsOrders, runs: 12);
    }

    private static List<Order> BuildTemplateOrders(List<Product> products, List<Customer> customers)
    {
        var o1 = SampleData.GetOrders(products, customers).First(o => o.Items.Count > 0);
        var o2 = SampleData.GetOrders(products, customers).Skip(1).First(o => o.Items.Count > 0);
        var o3 = CloneOrder(SampleData.GetOrders(products, customers).First(o => o.Items.Count > 0));
        o3.Items[0].Quantity = 0;
        var o4 = SampleData.GetOrders(products, customers).Skip(3).First(o => o.Items.Count > 0);
        var o5 = CloneOrder(o4);
        foreach (var i in o5.Items)
            i.UnitPrice = -1;
        var o6 = SampleData.GetOrders(products, customers).Skip(4).First(o => o.Items.Count > 0);

        foreach (var o in new[] { o1, o2, o3, o4, o5, o6 })
            o.Status = OrderStatus.Completed;

        return new List<Order> { o1, o2, o3, o4, o5, o6 };
    }

    private static Order CloneOrder(Order source)
    {
        var o = new Order
        {
            Id = source.Id,
            CustomerId = source.CustomerId,
            Customer = source.Customer,
            OrderDate = source.OrderDate,
            Status = source.Status,
            Notes = source.Notes,
            Items = source.Items.Select(i => new OrderItem
            {
                Id = i.Id,
                OrderId = i.OrderId,
                ProductId = i.ProductId,
                Product = i.Product,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };
        return o;
    }

    private static IEnumerable<Order> CloneOrders(List<Order> template)
    {
        foreach (var t in template)
        {
            var c = CloneOrder(t);
            c.Id = Random.Shared.Next(10_000, 99_999);
            yield return c;
        }
    }
}
