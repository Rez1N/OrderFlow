namespace OrderFlow.Console.Models;

/// <summary>
/// Model klienta w systemie
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string? Email { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public bool IsVip { get; set; }

    public List<Order> Orders { get; set; } = new();

    public override string ToString() =>
        $"[{Id}] {FullName} ({Email}) - {City}, {Country}" +
        (IsVip ? " [VIP]" : "");
}
