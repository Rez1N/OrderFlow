using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Niestandardowy delegat do walidacji zamówień
/// </summary>
public delegate bool ValidationRule(Order order, out string errorMessage);

/// <summary>
/// Klasa odpowiadająca za walidację zamówień
/// Łączy dwa mechanizmy: custom delegate i Func<Order, bool>
/// </summary>
public class OrderValidator
{
    private readonly List<ValidationRule> _customRules = new();
    private readonly List<Func<Order, bool>> _funcRules = new();

    /// <summary>
    /// Rejestruje regułę walidacyjną z custom delegatem
    /// </summary>
    public void RegisterCustomRule(ValidationRule rule)
    {
        _customRules.Add(rule);
    }

    /// <summary>
    /// Rejestruje regułę walidacyjną z Func<>
    /// </summary>
    public void RegisterFuncRule(Func<Order, bool> rule)
    {
        _funcRules.Add(rule);
    }

    /// <summary>
    /// Reguła: Zamówienie musi mieć co najmniej jedną pozycję
    /// </summary>
    private bool MustHaveItems(Order order, out string errorMessage)
    {
        if (order.Items == null || order.Items.Count == 0)
        {
            errorMessage = "Zamówienie musi zawierać co najmniej jedną pozycję.";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Reguła: Kwota zamówienia nie może przekroczyć limitu (50000 zł)
    /// </summary>
    private bool TotalAmountNotExceedsLimit(Order order, out string errorMessage)
    {
        const decimal MAX_ORDER_AMOUNT = 50000m;
        if (order.TotalAmount > MAX_ORDER_AMOUNT)
        {
            errorMessage = $"Kwota zamówienia nie może przekroczyć {MAX_ORDER_AMOUNT} zł. Aktualna: {order.TotalAmount} zł";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Reguła: Wszystkie pozycje muszą mieć ilość większą niż 0
    /// </summary>
    private bool AllItemsHavePositiveQuantity(Order order, out string errorMessage)
    {
        var invalidItems = order.Items.Where(item => item.Quantity <= 0).ToList();
        if (invalidItems.Count > 0)
        {
            errorMessage = $"Znaleziono {invalidItems.Count} pozycji z niedodatnią ilością.";
            return false;
        }
        errorMessage = string.Empty;
        return true;
    }

    /// <summary>
    /// Waliduje zamówienie używając wszystkich zarejestrowanych reguł
    /// </summary>
    public ValidationResult ValidateAll(Order order)
    {
        var errors = new List<string>();

        // Walidacja custom delegate'ami
        foreach (var rule in _customRules)
        {
            if (!rule(order, out var errorMessage))
            {
                errors.Add(errorMessage);
            }
        }

        // Walidacja Func<Order, bool>
        foreach (var rule in _funcRules)
        {
            if (!rule(order))
            {
                errors.Add("Reguła Func nie spełniona");
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    /// <summary>
    /// Tworzy validator z domyślnymi regułami
    /// </summary>
    public static OrderValidator CreateWithDefaults()
    {
        var validator = new OrderValidator();

        // Rejestracja custom delegate reguł
        validator.RegisterCustomRule(validator.MustHaveItems);
        validator.RegisterCustomRule(validator.TotalAmountNotExceedsLimit);
        validator.RegisterCustomRule(validator.AllItemsHavePositiveQuantity);

        // Rejestracja Func<Order, bool> reguł (lambdy)
        validator.RegisterFuncRule(order =>
            order.OrderDate <= DateTime.Now); // Data nie może być z przyszłości

        validator.RegisterFuncRule(order =>
            order.Status != OrderStatus.Cancelled); // Status nie może być Cancelled

        return validator;
    }
}

/// <summary>
/// Wynik walidacji zamówienia
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public override string ToString()
    {
        if (IsValid)
            return "✓ Walidacja powiodła się!";

        return "✗ Błędy walidacji:\n" + string.Join("\n", Errors.Select(e => $"  - {e}"));
    }
}
