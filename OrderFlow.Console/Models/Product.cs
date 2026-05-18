using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

/// <summary>
/// Model produktu w systemie
/// </summary>
public class Product
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlElement("name")]
    public string Name { get; set; }

    [XmlElement("category")]
    public string Category { get; set; }

    [XmlElement("price")]
    public decimal Price { get; set; }

    [XmlElement("stock")]
    public int StockQuantity { get; set; }

    [XmlIgnore]
    public string Description { get; set; }

    public List<OrderItem> OrderItems { get; set; } = new();

    public override string ToString() =>
        $"[{Id}] {Name} ({Category}) - {Price}zł (Stock: {StockQuantity})";
}
