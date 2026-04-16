using OrderFlow.Console.Models;
using OrderFlow.Console.Services;
using OrderFlow.Console.Subscribers;

namespace OrderFlow.Console.Lab2;

/// <summary>Lab 2 — zadanie 1: zdarzenia w pipeline.</summary>
public static class Lab2Task1Events
{
    public static void Run()
    {
        System.Console.WriteLine("\n" + new string('=', 70));
        System.Console.WriteLine("LAB 2 — ZADANIE 1: Zdarzenia (EventArgs, subskrybenci)".PadRight(70));
        System.Console.WriteLine(new string('=', 70));

        var products = SampleData.GetProducts();
        var customers = SampleData.GetCustomers();
        var source = SampleData.GetOrders(products, customers).Take(3).ToList();

        foreach (var o in source)
            o.Status = OrderStatus.New;

        if (source.Count >= 3 && source[2].Items.Count > 0)
            source[2].Items[0].Quantity = source[2].Items[0].Product!.StockQuantity + 50;

        var pipeline = new OrderPipeline();
        var logger = new ConsoleStatusLogger();
        var email = new EmailNotificationSimulator();
        var stats = new PipelineStatisticsSubscriber();

        pipeline.StatusChanged += logger.OnStatusChanged;
        pipeline.StatusChanged += email.OnStatusChanged;
        pipeline.StatusChanged += stats.OnStatusChanged;
        pipeline.ValidationCompleted += email.OnValidationCompleted;
        pipeline.ValidationCompleted += stats.OnValidationCompleted;

        foreach (var order in source)
        {
            System.Console.WriteLine($"\n>>> Przetwarzanie zamówienia #{order.Id}");
            pipeline.ProcessOrder(order);
        }

        System.Console.WriteLine(
            $"\n[STATS] Zmian statusu: {stats.StatusChangeCount}, zakończonych walidacji: {stats.ValidationCount}");
    }
}
