using System;
using System.Collections.Generic;
using System.Linq;
using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Klasa przetwarzająca zamówienia z użyciem generycznych delegatów
/// </summary>
public class OrderProcessor
{
    /// <summary>
    /// Filtruje zamówienia wg predykatu
    /// Zwraca listę zamówień spełniających warunek
    /// </summary>
    public List<Order> FilterOrders(List<Order> orders, Predicate<Order> predicate)
    {
        return orders.FindAll(predicate);
    }

    /// <summary>
    /// Wykonuje akcję na każdym zamówieniu z listy
    /// </summary>
    public void ExecuteAction(List<Order> orders, Action<Order> action)
    {
        foreach (var order in orders)
        {
            action(order);
        }
    }

    /// <summary>
    /// Projektuje zamówienia na dowolny typ T
    /// </summary>
    public List<T> ProjectOrders<T>(List<Order> orders, Func<Order, T> projection)
    {
        return orders.Select(projection).ToList();
    }

    /// <summary>
    /// Agreguje zamówienia używając dostarczanej funkcji agregacji
    /// </summary>
    public decimal Aggregate(List<Order> orders, Func<IEnumerable<Order>, decimal> aggregator)
    {
        return aggregator(orders);
    }

    /// <summary>
    /// Łańcuch operacji: filtruje → sortuje → bierze top N → wykonuje akcję
    /// </summary>
    public void FilterSortAndExecute(
        List<Order> orders,
        Predicate<Order> filter,
        Func<Order, decimal> sortKey,
        int topCount,
        Action<Order> action)
    {
        var filtered = orders
            .FindAll(filter)
            .OrderByDescending(sortKey)
            .Take(topCount)
            .ToList();
        
        foreach (var order in filtered)
        {
            action(order);
        }
    }
}

/// <summary>
/// Rozszerzenia demonstracyjne do OrderProcessor
/// </summary>
public static class OrderProcessorExtensions
{
    /// <summary>
    /// Przykład 1: Predicate - filtrowanie zamówień z konkretnym statusem
    /// </summary>
    public static Predicate<Order> ByStatus(OrderStatus status)
    {
        return order => order.Status == status;
    }

    /// <summary>
    /// Przykład 2: Predicate - filtrowanie zamówień od VIP klientów
    /// </summary>
    public static Predicate<Order> FromVipCustomers()
    {
        return order => order.Customer?.IsVip == true;
    }

    /// <summary>
    /// Przykład 3: Predicate - filtrowanie dużych zamówień (>5000 zł)
    /// </summary>
    public static Predicate<Order> LargeOrders()
    {
        return order => order.TotalAmount > 5000m;
    }

    /// <summary>
    /// Przykład 1: Action - wypisanie zamówienia
    /// </summary>
    public static Action<Order> PrintOrder()
    {
        return order => System.Console.WriteLine(order);
    }

    /// <summary>
    /// Przykład 2: Action - zmiana statusu zamówienia
    /// </summary>
    public static Action<Order> ChangeStatus(OrderStatus newStatus)
    {
        return order => order.Status = newStatus;
    }

    /// <summary>
    /// Przykład 1: Func - projekcja na typ anonimowy
    /// </summary>
    public static Func<Order, object> ProjectToOrderSummary()
    {
        return order => new
        {
            OrderId = order.Id,
            CustomerName = order.Customer?.Name,
            Total = order.TotalAmount,
            ItemCount = order.Items.Count,
            Status = order.Status.ToString()
        };
    }

    /// <summary>
    /// Przykład 2: Func - projekcja na string
    /// </summary>
    public static Func<Order, string> ProjectToOrderInfo()
    {
        return order => $"Order#{order.Id}: {order.Customer?.Name} - {order.TotalAmount}zł ({order.Status})";
    }

    /// <summary>
    /// Agregator 1: Suma kwot wszystkich zamówień
    /// </summary>
    public static Func<IEnumerable<Order>, decimal> SumTotal()
    {
        return orders => orders.Sum(o => o.TotalAmount);
    }

    /// <summary>
    /// Agregator 2: Średnia wartość zamówienia
    /// </summary>
    public static Func<IEnumerable<Order>, decimal> AverageTotal()
    {
        return orders => orders.Any() ? orders.Average(o => o.TotalAmount) : 0m;
    }

    /// <summary>
    /// Agregator 3: Maksymalna wartość zamówienia
    /// </summary>
    public static Func<IEnumerable<Order>, decimal> MaxTotal()
    {
        return orders => orders.Any() ? orders.Max(o => o.TotalAmount) : 0m;
    }
}
