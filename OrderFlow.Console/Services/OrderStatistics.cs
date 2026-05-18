using System.Collections.Concurrent;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Lab 2 — zadanie 3: thread-safe statystyki (lock, Interlocked, ConcurrentDictionary).
/// </summary>
public sealed class OrderStatistics
{
    private int _totalProcessed;
    private decimal _totalRevenue;
    private readonly ConcurrentDictionary<OrderStatus, int> _ordersPerStatus = new();
    private readonly List<string> _processingErrors = new();
    private readonly object _moneyAndErrorsLock = new();

    public int TotalProcessed => _totalProcessed;
    public decimal TotalRevenue => _totalRevenue;
    public IReadOnlyDictionary<OrderStatus, int> OrdersPerStatus => _ordersPerStatus;
    public IReadOnlyList<string> ProcessingErrors => _processingErrors;

    public void Record(Order order, bool success)
    {
        Interlocked.Increment(ref _totalProcessed);

        if (success)
        {
            lock (_moneyAndErrorsLock)
            {
                _totalRevenue += order.TotalAmount;
            }

            _ordersPerStatus.AddOrUpdate(order.Status, 1, (_, v) => v + 1);
        }
        else
        {
            lock (_moneyAndErrorsLock)
            {
                _processingErrors.Add($"Zamówienie #{order.Id}: błąd walidacji / przetwarzania");
            }

            _ordersPerStatus.AddOrUpdate(OrderStatus.Failed, 1, (_, v) => v + 1);
        }
    }

    public string Summarize(string label)
    {
        var perStatus = string.Join(", ", _ordersPerStatus.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
        return $"{label} | processed={TotalProcessed}, revenue={TotalRevenue:F2}, statuses=[{perStatus}], errors={_processingErrors.Count}";
    }
}

/// <summary>Celowo niebezpieczna wersja pod demonstrację wyścigów (bez locków).</summary>
public sealed class OrderStatisticsUnsafe
{
    public int TotalProcessed;
    public decimal TotalRevenue;
    /// <summary>Indeks = (int)OrderStatus (enum ma wartości 1..6).</summary>
    public int[] StatusBuckets { get; } = new int[Enum.GetValues<OrderStatus>().Cast<int>().Max() + 1];
    public int ErrorEvents;

    public string Summarize(string label)
    {
        var parts = Enum.GetValues<OrderStatus>()
            .Cast<OrderStatus>()
            .Select(s => $"{s}={StatusBuckets[(int)s]}");
        var perStatus = string.Join(", ", parts);
        return $"{label} | processed={TotalProcessed}, revenue={TotalRevenue:F2}, statuses=[{perStatus}], errorEvents={ErrorEvents}";
    }
}

public static class OrderStatisticsDemos
{
    private static string MetricsPart(string summaryLine)
    {
        var i = summaryLine.IndexOf('|');
        return i >= 0 ? summaryLine[(i + 1)..].Trim() : summaryLine;
    }

    public static void RunUnsafeParallelDemo(List<Order> orders, int runs = 12)
    {
        System.Console.WriteLine("\n--- Wersja BEZ synchronizacji (Parallel.ForEach) ---");
        var summaries = new List<string>();
        for (var i = 0; i < runs; i++)
        {
            var stats = new OrderStatisticsUnsafe();
            Parallel.ForEach(orders, order =>
            {
                var ok = order.Items.Count > 0 && order.TotalAmount > 0 && order.Items.All(x => x.Quantity > 0);
                stats.TotalProcessed++;
                if (ok)
                {
                    stats.TotalRevenue += order.TotalAmount;
                    stats.StatusBuckets[(int)order.Status]++;
                }
                else
                {
                    stats.ErrorEvents++;
                    stats.StatusBuckets[(int)OrderStatus.Failed]++;
                }
            });
            var s = stats.Summarize($"Run {i + 1}");
            System.Console.WriteLine(s);
            summaries.Add(s);
        }

        var refMetrics = MetricsPart(summaries[0]);
        var allSame = summaries.TrueForAll(s =>
            string.Equals(MetricsPart(s), refMetrics, StringComparison.Ordinal));
        var distinct = summaries.Select(MetricsPart).Distinct(StringComparer.Ordinal).Count();
        System.Console.WriteLine(allSame
            ? "Metryki przyjmują te same wartości (rzadkie przy wyścigach)."
            : $"Wyniki różnią się ({distinct} unikalnych zestawów metryk) — race conditions.");
    }

    public static void RunSafeParallelDemo(List<Order> orders, int runs = 12)
    {
        System.Console.WriteLine("\n--- Wersja z lock / Interlocked / ConcurrentDictionary ---");
        var summaries = new List<string>();
        for (var i = 0; i < runs; i++)
        {
            var stats = new OrderStatistics();
            Parallel.ForEach(orders, order =>
            {
                var ok = order.Items.Count > 0 && order.TotalAmount > 0 && order.Items.All(x => x.Quantity > 0);
                stats.Record(order, ok);
            });
            var s = stats.Summarize($"Run {i + 1}");
            System.Console.WriteLine(s);
            summaries.Add(s);
        }

        var refMetrics = MetricsPart(summaries[0]);
        var allSame = summaries.TrueForAll(s =>
            string.Equals(MetricsPart(s), refMetrics, StringComparison.Ordinal));
        System.Console.WriteLine(allSame
            ? "Po naprawie wszystkie uruchomienia dają ten sam wynik."
            : "Niespodziewany rozrzut — sprawdź implementację.");
    }
}
