using System.Diagnostics;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>Lab 2 — zadanie 2: symulacja usług zewnętrznych (async).</summary>
public class ExternalServiceSimulator
{
    public async Task<bool> CheckInventoryAsync(Product product)
    {
        await Task.Delay(Random.Shared.Next(500, 1501));
        return product.StockQuantity > 0;
    }

    public async Task<bool> ValidatePaymentAsync(Order order)
    {
        await Task.Delay(Random.Shared.Next(1000, 2001));
        return order.TotalAmount > 0;
    }

    public async Task<decimal> CalculateShippingAsync(Order order)
    {
        await Task.Delay(Random.Shared.Next(300, 801));
        var units = order.Items.Sum(i => i.Quantity);
        return Math.Round(5m + units * 0.5m, 2);
    }

    public async Task ProcessOrderAsync(Order order)
    {
        var firstProduct = order.Items.FirstOrDefault()?.Product
            ?? throw new InvalidOperationException("Brak produktu w zamówieniu.");
        var sw = Stopwatch.StartNew();
        var inventoryTask = CheckInventoryAsync(firstProduct);
        var paymentTask = ValidatePaymentAsync(order);
        var shippingTask = CalculateShippingAsync(order);
        await Task.WhenAll(inventoryTask, paymentTask, shippingTask);
        sw.Stop();
        System.Console.WriteLine(
            $"  ProcessOrderAsync(#{order.Id}): równoległe wywołania zakończone w {sw.ElapsedMilliseconds} ms");
    }

    public async Task ProcessMultipleOrdersAsync(List<Order> orders, int maxConcurrency = 3)
    {
        using var gate = new SemaphoreSlim(maxConcurrency, maxConcurrency);
        var processed = 0;
        var lockObj = new object();
        var total = orders.Count;

        var tasks = orders.Select(async order =>
        {
            await gate.WaitAsync().ConfigureAwait(false);
            try
            {
                await ProcessOrderAsync(order).ConfigureAwait(false);
            }
            finally
            {
                gate.Release();
            }

            int done;
            lock (lockObj)
            {
                processed++;
                done = processed;
            }

            System.Console.WriteLine($"  Przetworzono {done}/{total} zamówień");
        });

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
