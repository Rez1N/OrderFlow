using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

/// <summary>
/// Model pozycji zamówienia
/// </summary>
public class OrderItem
{
    [XmlAttribute("id")]
    public int Id { get; set; }
    [XmlAttribute("orderId")]
    public int OrderId { get; set; }
    [XmlAttribute("productId")]
    public int ProductId { get; set; }

    [XmlElement("product")]
    public Product Product { get; set; }

    [XmlElement("quantity")]
    public int Quantity { get; set; }
    [XmlElement("unitPrice")]
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Cena całkowita pozycji (Quantity * UnitPrice)
    /// </summary>
    public decimal TotalPrice => Quantity * UnitPrice;

    public override string ToString() =>
        $"  - {Product?.Name} x{Quantity} @ {UnitPrice}zł = {TotalPrice}zł";
}
