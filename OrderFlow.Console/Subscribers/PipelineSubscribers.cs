using System.Threading;
using OrderFlow.Console.Events;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Subscribers;

public sealed class ConsoleStatusLogger
{
    public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        System.Console.WriteLine(
            $"[LOG] {e.Timestamp:O} | Zamówienie #{e.Order.Id} | {e.OldStatus} -> {e.NewStatus}");
    }
}

public sealed class EmailNotificationSimulator
{
    public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e)
    {
        if (e.NewStatus != OrderStatus.Completed)
            return;
        var email = e.Order.Customer?.Email ?? "(brak e-mail)";
        System.Console.WriteLine(
            $"[EMAIL] -> {email}: Zamówienie #{e.Order.Id} zostało zrealizowane.");
    }

    public void OnValidationCompleted(object? sender, OrderValidationEventArgs e)
    {
        var email = e.Order.Customer?.Email ?? "(brak e-mail)";
        var result = e.IsValid ? "zaliczyła walidację" : "nie przeszło walidacji";
        System.Console.WriteLine($"[EMAIL] -> {email}: Zamówienie #{e.Order.Id} {result}.");
    }
}

public sealed class PipelineStatisticsSubscriber
{
    private int _statusChanges;
    private int _validations;

    public int StatusChangeCount => _statusChanges;
    public int ValidationCount => _validations;

    public void OnStatusChanged(object? sender, OrderStatusChangedEventArgs e) =>
        Interlocked.Increment(ref _statusChanges);

    public void OnValidationCompleted(object? sender, OrderValidationEventArgs e) =>
        Interlocked.Increment(ref _validations);
}
