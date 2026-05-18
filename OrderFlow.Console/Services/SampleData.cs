using OrderFlow.Console.Models;

namespace OrderFlow.Console.Services;

/// <summary>
/// Klasa przygotowująca przykładowe dane do testów
/// </summary>
public static class SampleData
{
    /// <summary>
    /// Lista 5 produktów z różnych kategorii
    /// </summary>
    public static List<Product> GetProducts() => new()
    {
        new Product
        {
            Id = 1,
            Name = "Laptop Dell XPS 13",
            Category = "Elektronika",
            Price = 5999.99m,
            StockQuantity = 15,
            Description = "Ultrabook z ekranem 13 cali"
        },
        new Product
        {
            Id = 2,
            Name = "Mysz Logitech MX Master",
            Category = "Akcesoria",
            Price = 449.99m,
            StockQuantity = 50,
            Description = "Bezprzewodowa mysz dla profesjonalistów"
        },
        new Product
        {
            Id = 3,
            Name = "Monitor LG 27\"",
            Category = "Elektronika",
            Price = 1299.99m,
            StockQuantity = 20,
            Description = "4K monitor do biura"
        },
        new Product
        {
            Id = 4,
            Name = "Kabel USB-C 2m",
            Category = "Akcesoria",
            Price = 79.99m,
            StockQuantity = 200,
            Description = "Certyfikowany kabel USB-C"
        },
        new Product
        {
            Id = 5,
            Name = "Słuchawki Sony WH-1000",
            Category = "Audio",
            Price = 899.99m,
            StockQuantity = 30,
            Description = "Słuchawki z aktywną redukcją szumów"
        }
    };

    /// <summary>
    /// Lista 4 klientów, w tym 1 VIP
    /// </summary>
    public static List<Customer> GetCustomers() => new()
    {
        new Customer
        {
            Id = 1,
            FullName = "Jan Kowalski",
            Email = "jan.kowalski@email.com",
            City = "Warszawa",
            Country = "Polska",
            IsVip = false
        },
        new Customer
        {
            Id = 2,
            FullName = "Maria Lewandowska",
            Email = "maria.lewandowska@email.com",
            City = "Kraków",
            Country = "Polska",
            IsVip = true
        },
        new Customer
        {
            Id = 3,
            FullName = "Piotr Nowak",
            Email = "piotr.nowak@email.com",
            City = "Wrocław",
            Country = "Polska",
            IsVip = false
        },
        new Customer
        {
            Id = 4,
            FullName = "Anna Zielińska",
            Email = "anna.zielinska@email.com",
            City = "Gdańsk",
            Country = "Polska",
            IsVip = false
        }
    };

    /// <summary>
    /// Lista 6 zamówień z zróżnicowanymi danymi testowymi
    /// </summary>
    public static List<Order> GetOrders(List<Product> products, List<Customer> customers)
    {
        return new()
        {
            new Order
            {
                Id = 1,
                CustomerId = 1,
                Customer = customers.First(c => c.Id == 1),
                OrderDate = DateTime.Now.AddDays(-10),
                Status = OrderStatus.Completed,
                Notes = "Zamówienie dla siebie",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 1,
                        OrderId = 1,
                        ProductId = 1,
                        Product = products.First(p => p.Id == 1),
                        Quantity = 1,
                        UnitPrice = 5999.99m
                    },
                    new OrderItem
                    {
                        Id = 2,
                        OrderId = 1,
                        ProductId = 2,
                        Product = products.First(p => p.Id == 2),
                        Quantity = 2,
                        UnitPrice = 449.99m
                    }
                }
            },
            new Order
            {
                Id = 2,
                CustomerId = 2,
                Customer = customers.First(c => c.Id == 2),
                OrderDate = DateTime.Now.AddDays(-5),
                Status = OrderStatus.Processing,
                Notes = "VIP klient - priorytet!",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 3,
                        OrderId = 2,
                        ProductId = 3,
                        Product = products.First(p => p.Id == 3),
                        Quantity = 1,
                        UnitPrice = 1299.99m
                    },
                    new OrderItem
                    {
                        Id = 4,
                        OrderId = 2,
                        ProductId = 5,
                        Product = products.First(p => p.Id == 5),
                        Quantity = 1,
                        UnitPrice = 899.99m
                    }
                }
            },
            new Order
            {
                Id = 3,
                CustomerId = 3,
                Customer = customers.First(c => c.Id == 3),
                OrderDate = DateTime.Now.AddDays(-3),
                Status = OrderStatus.Validated,
                Notes = "Drobne akcesoria",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 5,
                        OrderId = 3,
                        ProductId = 4,
                        Product = products.First(p => p.Id == 4),
                        Quantity = 5,
                        UnitPrice = 79.99m
                    }
                }
            },
            new Order
            {
                Id = 4,
                CustomerId = 1,
                Customer = customers.First(c => c.Id == 1),
                OrderDate = DateTime.Now.AddDays(-1),
                Status = OrderStatus.New,
                Notes = "Drugie zamówienie",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 6,
                        OrderId = 4,
                        ProductId = 2,
                        Product = products.First(p => p.Id == 2),
                        Quantity = 1,
                        UnitPrice = 449.99m
                    }
                }
            },
            new Order
            {
                Id = 5,
                CustomerId = 4,
                Customer = customers.First(c => c.Id == 4),
                OrderDate = DateTime.Now.AddDays(-7),
                Status = OrderStatus.Cancelled,
                Notes = "Anulowane - zmiana zdania",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 7,
                        OrderId = 5,
                        ProductId = 5,
                        Product = products.First(p => p.Id == 5),
                        Quantity = 2,
                        UnitPrice = 899.99m
                    }
                }
            },
            new Order
            {
                Id = 6,
                CustomerId = 2,
                Customer = customers.First(c => c.Id == 2),
                OrderDate = DateTime.Now.AddDays(1), // Przyszłość - dla testów walidacji
                Status = OrderStatus.New,
                Notes = "VIP - duże zamówienie",
                Items = new()
                {
                    new OrderItem
                    {
                        Id = 8,
                        OrderId = 6,
                        ProductId = 1,
                        Product = products.First(p => p.Id == 1),
                        Quantity = 2,
                        UnitPrice = 5999.99m
                    },
                    new OrderItem
                    {
                        Id = 9,
                        OrderId = 6,
                        ProductId = 3,
                        Product = products.First(p => p.Id == 3),
                        Quantity = 3,
                        UnitPrice = 1299.99m
                    }
                }
            }
        };
    }
}
