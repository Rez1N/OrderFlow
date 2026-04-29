using OrderFlow.Console.Events;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Lab 2 — pipeline ze zdarzeniami: walidacja, potem stany New → Validated → Processing → Completed.
/// </summary>
public class OrderPipeline
{
    public event EventHandler<OrderStatusChangedEventArgs>? StatusChanged;
    public event EventHandler<OrderValidationEventArgs>? ValidationCompleted;

    public void ProcessOrder(Order order)
    {
        var validationErrors = Validate(order);
        var isValid = validationErrors.Count == 0;
        ValidationCompleted?.Invoke(this, new OrderValidationEventArgs(order, isValid, validationErrors));

        if (!isValid)
        {
            SetStatus(order, OrderStatus.Failed);
            return;
        }

        SetStatus(order, OrderStatus.Validated);
        SetStatus(order, OrderStatus.Processing);
        SetStatus(order, OrderStatus.Completed);
    }

    public Task ProcessOrderAsync(Order order)
    {
        ProcessOrder(order);
        return Task.CompletedTask;
    }

    private static List<string> Validate(Order order)
    {
        var errors = new List<string>();
        if (order.Status == OrderStatus.Cancelled)
            errors.Add("Zamówienie jest anulowane.");
        if (order.Items is not { Count: > 0 })
            errors.Add("Brak pozycji w zamówieniu.");
        else
        {
            foreach (var item in order.Items)
            {
                if (item.Quantity <= 0)
                    errors.Add($"Nieprawidłowa ilość dla produktu #{item.ProductId}.");
                if (item.Product is null)
                {
                    errors.Add("Brak referencji do produktu w pozycji.");
                    continue;
                }

                if (item.Quantity > item.Product.StockQuantity)
                    errors.Add($"Za mało stanu magazynowego: {item.Product.Name} (wymagane {item.Quantity}, dostępne {item.Product.StockQuantity}).");
            }
        }

        if (order.TotalAmount <= 0)
            errors.Add("Kwota zamówienia musi być dodatnia.");

        return errors;
    }

    private void SetStatus(Order order, OrderStatus newStatus)
    {
        var old = order.Status;
        if (old == newStatus)
            return;
        order.Status = newStatus;
        StatusChanged?.Invoke(this, new OrderStatusChangedEventArgs(order, old, newStatus, DateTimeOffset.UtcNow));
    }
}
