namespace OrderFlow.Console.Models;

/// <summary>
/// Model pozycji zamówienia
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Cena całkowita pozycji (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    public override string ToString() =>
        $"  - {Product?.Name} x{Quantity} @ {UnitPrice}zł = {TotalPrice}zł";
}
