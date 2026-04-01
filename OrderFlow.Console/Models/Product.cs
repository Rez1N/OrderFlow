namespace OrderFlow.Console.Models;

/// <summary>
/// Model produktu w systemie
/// </summary>
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Description { get; set; }

    public override string ToString() =>
        $"[{Id}] {Name} ({Category}) - {Price}zł (Stock: {StockQuantity})";
}
