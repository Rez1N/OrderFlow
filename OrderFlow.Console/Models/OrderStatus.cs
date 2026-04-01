namespace OrderFlow.Console.Models;

/// <summary>
/// Enum definiujący możliwe statusy zamówienia
/// </summary>
public enum OrderStatus
{
    New = 1,
    Validated = 2,
    Processing = 3,
    Completed = 4,
    Cancelled = 5
}
