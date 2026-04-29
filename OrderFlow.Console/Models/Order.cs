using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace OrderFlow.Console.Models;

/// <summary>
/// Model zamówienia w systemie
/// </summary>
public class Order
{
    [XmlAttribute("id")]
    public int Id { get; set; }

    [XmlAttribute("customerId")]
    public int CustomerId { get; set; }

    [JsonIgnore]
    [XmlIgnore]
    public Customer Customer { get; set; }

    [XmlElement("orderDate")]
    public DateTime OrderDate { get; set; }

    [XmlAttribute("status")]
    public OrderStatus Status { get; set; }

    [XmlArray("items")]
    [XmlArrayItem("item")]
    public List<OrderItem> Items { get; set; } = new();

    [JsonPropertyName("internalNotes")]
    [XmlElement("notes_text")]
    public string Notes { get; set; }

    /// <summary>
    /// Kwota całkowita zamówienia (suma TotalPrice wszystkich pozycji)
    /// </summary>
    public decimal TotalAmount => Items.Sum(item => item.TotalPrice);

    public override string ToString() =>
        $"Order #{Id} ({Status}) - {Customer?.Name} - {OrderDate:yyyy-MM-dd} - {TotalAmount}zł\n" +
        string.Join("\n", Items);
}
