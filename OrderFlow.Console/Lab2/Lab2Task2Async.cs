using System.Diagnostics;
using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

namespace OrderFlow.Console.Lab2;

/// <summary>Lab 2 — zadanie 2: async/await, Task.WhenAll, SemaphoreSlim.</summary>
public static class Lab2Task2Async
{
    public static async Task RunAsync(List<Product> products, List<Customer> customers)
    {
        System.Console.WriteLine("\n" + new string('=', 70));
        System.Console.WriteLine("LAB 2 — ZADANIE 2: Asynchroniczne pobieranie danych".PadRight(70));
        System.Console.WriteLine(new string('=', 70));

        var simulator = new ExternalServiceSimulator();
        var allOrders = SampleData.GetOrders(products, customers);

        var one = allOrders.First(o => o.Items.Count > 0);
        System.Console.WriteLine("\nPojedyncze zamówienie (3 serwisy równolegle — Task.WhenAll):");
        await simulator.ProcessOrderAsync(one);

        var batch = allOrders.Take(6).ToList();
        System.Console.WriteLine("\nPorównanie: sekwencyjnie vs równolegle (max 3 zamówienia naraz):");

        var swSeq = Stopwatch.StartNew();
        foreach (var o in batch)
            await simulator.ProcessOrderAsync(o);
        swSeq.Stop();
        System.Console.WriteLine($"Sekwencyjnie (6 zamówień): {swSeq.ElapsedMilliseconds} ms");

        var batch2 = SampleData.GetOrders(products, customers).Take(6).ToList();
        var swPar = Stopwatch.StartNew();
        await simulator.ProcessMultipleOrdersAsync(batch2, maxConcurrency: 3);
        swPar.Stop();
        System.Console.WriteLine($"Równolegle (SemaphoreSlim = 3): {swPar.ElapsedMilliseconds} ms");
    }
}
