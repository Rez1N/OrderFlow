using OrderFlow.Console.Models;

namespace OrderFlow.Console.Events;

public class OrderValidationEventArgs : EventArgs
{
    public Order Order { get; }
    public bool IsValid { get; }
    public IReadOnlyList<string> Errors { get; }

    public OrderValidationEventArgs(Order order, bool isValid, IReadOnlyList<string> errors)
    {
        Order = order;
        IsValid = isValid;
        Errors = errors;
    }
}
