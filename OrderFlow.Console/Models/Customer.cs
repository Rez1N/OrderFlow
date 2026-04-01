namespace OrderFlow.Console.Models;

/// <summary>
/// Model klienta w systemie
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public bool IsVip { get; set; }

    public override string ToString() =>
        $"[{Id}] {Name} ({Email}) - {City}, {Country}" +
        (IsVip ? " [VIP]" : "");
}
