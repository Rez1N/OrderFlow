using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Models;
 
namespace OrderFlow.Console.Persistence;
 
public static class DatabaseSeeder
{
    public static async Task SeedAsync(OrderFlowContext db)
    {
        // Sprawdź czy baza jest pusta
        if (await db.Customers.AnyAsync())
        {
            System.Console.WriteLine("[Seeder] Baza już zawiera dane — pomijam seedowanie.");
            return;
        }
 
        System.Console.WriteLine("[Seeder] Seedowanie bazy danych...");
 
        // ── Produkty ──────────────────────────────────────────────────────────
        var products = new List<Product>
        {
            new() { Name = "Laptop Dell XPS 15",     Category = "Elektronika",   Price = 5499.99m, StockQuantity = 10, Description = "Laptop wysokiej klasy" },
            new() { Name = "Słuchawki Sony WH-1000XM5", Category = "Elektronika", Price = 1299.99m, StockQuantity = 25, Description = "Bezprzewodowe słuchawki ANC" },
            new() { Name = "Klawiatura Logitech MX Keys", Category = "Akcesoria", Price = 449.99m,  StockQuantity = 50, Description = "Klawiatura mechaniczna" },
            new() { Name = "Monitor LG 27\" 4K",      Category = "Elektronika",   Price = 2199.99m, StockQuantity = 8,  Description = "Monitor UHD IPS" },
            new() { Name = "Mysz Logitech MX Master 3", Category = "Akcesoria",  Price = 349.99m,  StockQuantity = 0,  Description = "Ergonomiczna mysz" },
            new() { Name = "Dysk SSD Samsung 1TB",   Category = "Komponenty",    Price = 399.99m,  StockQuantity = 30, Description = "SSD NVMe M.2" },
        };
 
        await db.Products.AddRangeAsync(products);
        await db.SaveChangesAsync(); // Produkty dostają Id
 
        // ── Klienci ───────────────────────────────────────────────────────────
        var customers = new List<Customer>
        {
            new() { FullName = "Anna Kowalska",   Email = "anna@example.com",   City = "Warszawa", Country = "Polska",  IsVip = false },
            new() { FullName = "Piotr Nowak",     Email = "piotr@example.com",  City = "Kraków",   Country = "Polska",  IsVip = true  },
            new() { FullName = "Maria Wiśniewska",Email = "maria@example.com",  City = "Wrocław",  Country = "Polska",  IsVip = false },
            new() { FullName = "Jan Kowalczyk",   Email = "jan@example.com",    City = "Gdańsk",   Country = "Polska",  IsVip = true  },
        };
 
        await db.Customers.AddRangeAsync(customers);
        await db.SaveChangesAsync(); // Klienci dostają Id
 
        // ── Zamówienia ────────────────────────────────────────────────────────
        var orders = new List<Order>
        {
            new()
            {
                CustomerId  = customers[0].Id,
                OrderDate   = DateTime.Now.AddDays(-10),
                Status      = OrderStatus.Completed,
                Notes       = "Szybka dostawa",
                Items       =
                [
                    new() { ProductId = products[0].Id, Quantity = 1, UnitPrice = products[0].Price },
                    new() { ProductId = products[2].Id, Quantity = 2, UnitPrice = products[2].Price },
                ]
            },
            new()
            {
                CustomerId  = customers[1].Id, // VIP
                OrderDate   = DateTime.Now.AddDays(-7),
                Status      = OrderStatus.Processing,
                Notes       = "Klient VIP — priorytet",
                Items       =
                [
                    new() { ProductId = products[1].Id, Quantity = 1, UnitPrice = products[1].Price },
                    new() { ProductId = products[3].Id, Quantity = 1, UnitPrice = products[3].Price },
                ]
            },
            new()
            {
                CustomerId  = customers[2].Id,
                OrderDate   = DateTime.Now.AddDays(-5),
                Status      = OrderStatus.New,
                Items       =
                [
                    new() { ProductId = products[5].Id, Quantity = 2, UnitPrice = products[5].Price },
                ]
            },
            new()
            {
                CustomerId  = customers[3].Id, // VIP
                OrderDate   = DateTime.Now.AddDays(-3),
                Status      = OrderStatus.Completed,
                Notes       = "Ekspresowa realizacja",
                Items       =
                [
                    new() { ProductId = products[0].Id, Quantity = 2, UnitPrice = products[0].Price },
                    new() { ProductId = products[3].Id, Quantity = 1, UnitPrice = products[3].Price },
                ]
            },
            new()
            {
                CustomerId  = customers[1].Id, // VIP
                OrderDate   = DateTime.Now.AddDays(-2),
                Status      = OrderStatus.New,
                Notes       = "Pilne zamówienie VIP",
                Items       =
                [
                    new() { ProductId = products[4].Id, Quantity = 3, UnitPrice = products[4].Price },
                    new() { ProductId = products[2].Id, Quantity = 1, UnitPrice = products[2].Price },
                ]
            },
            new()
            {
                CustomerId  = customers[0].Id,
                OrderDate   = DateTime.Now.AddDays(-1),
                Status      = OrderStatus.Cancelled,
                Notes       = "Anulowane przez klienta",
                Items       =
                [
                    new() { ProductId = products[1].Id, Quantity = 1, UnitPrice = products[1].Price },
                ]
            },
        };
 
        await db.Orders.AddRangeAsync(orders);
        await db.SaveChangesAsync();
 
        System.Console.WriteLine($"[Seeder] Dodano: {products.Count} produktów, {customers.Count} klientów, {orders.Count} zamówień.");
    }
}
 