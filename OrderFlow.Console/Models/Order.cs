namespace OrderFlow.Console.Models;

/// <summary>
/// Model zamówienia w systemie
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public string Notes { get; set; }

    /// <summary>
    /// Kwota całkowita zamówienia (suma TotalPrice wszystkich pozycji)
    /// </summary>
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);

    public override string ToString() =>
        $"Order #{Id} ({Status}) - {Customer?.Name} - {OrderDate:yyyy-MM-dd} - {TotalAmount}zł\n" +
        string.Join("\n", Items);
}
