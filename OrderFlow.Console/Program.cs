using Microsoft.EntityFrameworkCore;
using OrderFlow.Console.Lab2;
using OrderFlow.Console.Models;
using OrderFlow.Console.Persistence;
using OrderFlow.Console.Services;
using OrderFlow.Console.Watchers;

var products = SampleData.GetProducts();
var customers = SampleData.GetCustomers();
var orders = SampleData.GetOrders(products, customers);

PrintHeader("ZADANIE 1 - Model domenowy i dane testowe");
Console.WriteLine("\n=== PRODUKTY ===");
foreach (var product in products)
    Console.WriteLine(product);

Console.WriteLine("\n=== KLIENCI ===");
foreach (var customer in customers)
    Console.WriteLine(customer);

Console.WriteLine("\n=== ZAMÓWIENIA ===");
foreach (var order in orders)
    Console.WriteLine($"{order}\n");

// ============================================================================
// ZADANIE 2 - Delegaty i walidacja zamówień
// ============================================================================
PrintHeader("ZADANIE 2 - Delegaty i walidacja zamówień");

var validator = OrderValidator.CreateWithDefaults();

Console.WriteLine("\n--- Walidacja zamówienia POPRAWNEGO (ID=1) ---");
var validOrder = orders.First(o => o.Id == 1);
var result = validator.ValidateAll(validOrder);
Console.WriteLine(result);

Console.WriteLine("\n--- Walidacja zamówienia BŁĘDNEGO (ID=6 - z przyszłości) ---");
var invalidOrder = orders.First(o => o.Id == 6);
result = validator.ValidateAll(invalidOrder);
Console.WriteLine(result);

// ============================================================================
// ZADANIE 3 - Action, Func, Predicate
// ============================================================================
PrintHeader("ZADANIE 3 - Action, Func, Predicate");

var processor = new OrderProcessor();

Console.WriteLine("\n--- Filtry Predicate (3 różne) ---");

var completedOrders = processor.FilterOrders(orders, OrderProcessorExtensions.ByStatus(OrderStatus.Completed));
Console.WriteLine($"\n1. Zamówienia skompletowane ({completedOrders.Count}):");
completedOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.FullName} - {o.TotalAmount}zł"));

var vipOrders = processor.FilterOrders(orders, OrderProcessorExtensions.FromVipCustomers());
Console.WriteLine($"\n2. Zamówienia od VIP klientów ({vipOrders.Count}):");
vipOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.FullName} - {o.TotalAmount}zł"));

var largeOrders = processor.FilterOrders(orders, OrderProcessorExtensions.LargeOrders());
Console.WriteLine($"\n3. Duże zamówienia (>5000zł) ({largeOrders.Count}):");
largeOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.FullName} - {o.TotalAmount}zł"));

Console.WriteLine("\n--- Akcje Action (2 zastosowania) ---");

Console.WriteLine("\n1. Wypisywanie zamówień z statusem 'Processing':");
var processingOrders = processor.FilterOrders(orders, OrderProcessorExtensions.ByStatus(OrderStatus.Processing));
processor.ExecuteAction(processingOrders, OrderProcessorExtensions.PrintOrder());

Console.WriteLine("\n2. Zmiana statusu zamówień na 'Completed':");
var testOrders = new List<Order> { orders[2], orders[3] };
Console.WriteLine("  Przed zmianą:");
testOrders.ForEach(o => Console.WriteLine($"    #{o.Id}: {o.Status}"));
processor.ExecuteAction(testOrders, OrderProcessorExtensions.ChangeStatus(OrderStatus.Completed));
Console.WriteLine("  Po zmianie:");
testOrders.ForEach(o => Console.WriteLine($"    #{o.Id}: {o.Status}"));

Console.WriteLine("\n--- Projekcja Func na typ anonimowy ---");
var summaries = processor.ProjectOrders(orders.Take(3).ToList(), OrderProcessorExtensions.ProjectToOrderSummary());
foreach (var summary in summaries)
    Console.WriteLine($"  {summary}");

Console.WriteLine("\n--- Agregacja (3 agregatory) ---");
Console.WriteLine($"1. Suma kwot: {processor.Aggregate(orders, OrderProcessorExtensions.SumTotal()):C}");
Console.WriteLine($"2. Średnia kwota: {processor.Aggregate(orders, OrderProcessorExtensions.AverageTotal()):C}");
Console.WriteLine($"3. Maksymalna kwota: {processor.Aggregate(orders, OrderProcessorExtensions.MaxTotal()):C}");

Console.WriteLine("\n--- Łańcuch operacji: filtruj → sortuj → top 2 → wypisz ---");
processor.FilterSortAndExecute(
    orders,
    o => o.TotalAmount > 1000m,
    o => o.TotalAmount,
    2,
    o => Console.WriteLine($"  #{o.Id}: {o.Customer?.FullName} - {o.TotalAmount}zł")
);

testOrders[0].Status = OrderStatus.Validated;
testOrders[1].Status = OrderStatus.New;

// ============================================================================
// ZADANIE 4 - LINQ
// ============================================================================
PrintHeader("ZADANIE 4 - LINQ");

Console.WriteLine("\n--- Q1: GroupBy + agregacja (Method Syntax) ---");
var q1 = orders
    .GroupBy(o => o.Customer)
    .Select(g => new
    {
        Customer    = g.Key.FullName,
        OrderCount  = g.Count(),
        TotalAmount = g.Sum(o => o.TotalAmount),
        AverageOrder = g.Average(o => o.TotalAmount)
    })
    .OrderByDescending(x => x.TotalAmount)
    .Take(3);
foreach (var item in q1)
    Console.WriteLine($"  {item.Customer}: {item.OrderCount} zamówień, razem {item.TotalAmount}zł (śr. {item.AverageOrder:F0}zł)");

Console.WriteLine("\n--- Q2: Join (Query Syntax) ---");
var q2 = from order in orders
         join customer in customers on order.CustomerId equals customer.Id
         group new { order, customer } by customer.City into cityGroup
         select new
         {
             City        = cityGroup.Key,
             OrderCount  = cityGroup.Count(),
             TotalAmount = cityGroup.Sum(x => x.order.TotalAmount)
         };
foreach (var item in q2.OrderByDescending(x => x.TotalAmount))
    Console.WriteLine($"  {item.City}: {item.OrderCount} zamówień, {item.TotalAmount}zł");

Console.WriteLine("\n--- Q3: SelectMany (Method Syntax) ---");
var q3 = orders.SelectMany(o => o.Items).Select(i => i.Product).Distinct().OrderBy(p => p.Name);
foreach (var product in q3)
    Console.WriteLine($"  {product.Name} ({product.Category})");

Console.WriteLine("\n--- Q4: GroupBy (Query Syntax) ---");
var q4 = from product in products
         group product by product.Category into catGroup
         select new
         {
             Category     = catGroup.Key,
             ProductCount = catGroup.Count(),
             AvgPrice     = catGroup.Average(p => p.Price)
         };
foreach (var item in q4.OrderByDescending(x => x.AvgPrice))
    Console.WriteLine($"  {item.Category}: {item.ProductCount} produktów, śr. cena {item.AvgPrice:F2}zł");

Console.WriteLine("\n--- Q5: GroupJoin (Method Syntax - Left Join Pattern) ---");
var q5 = customers
    .GroupJoin(orders, customer => customer.Id, order => order.CustomerId,
        (customer, customerOrders) => new
        {
            CustomerName = customer.FullName,
            IsVip        = customer.IsVip,
            OrderCount   = customerOrders.Count(),
            TotalSpent   = customerOrders.Sum(o => o.TotalAmount)
        })
    .OrderByDescending(x => x.TotalSpent);
foreach (var item in q5)
    Console.WriteLine($"  {item.CustomerName} {(item.IsVip ? "[VIP]" : "")}: {item.OrderCount} zamówień, razem {item.TotalSpent:F0}zł");

Console.WriteLine("\n--- Q6: Mixed Syntax ---");
var q6 = from customer in customers
         let customerOrders = orders.Where(o => o.CustomerId == customer.Id)
         let favoriteCategory = customerOrders
             .SelectMany(o => o.Items)
             .GroupBy(i => i.Product.Category)
             .OrderByDescending(g => g.Count())
             .Select(g => g.Key)
             .FirstOrDefault() ?? "Brak"
         select new
         {
             Customer         = customer.FullName,
             OrderCount       = customerOrders.Count(),
             TotalAmount      = customerOrders.Sum(o => o.TotalAmount),
             FavoriteCategory = favoriteCategory
         } into report
         orderby report.TotalAmount descending
         select report;
foreach (var item in q6)
    Console.WriteLine($"  {item.Customer}: {item.OrderCount} zamówień, {item.TotalAmount}zł, ulub. kategoria: {item.FavoriteCategory}");

Console.WriteLine();
PrintHeader("Projekt OrderFlow - koniec demonstracji Lab 1");

// ============================================================================
// LAB 2
// ============================================================================
Lab2Task1Events.Run();
await Lab2Task2Async.RunAsync(products, customers);
Lab2Task3Statistics.Run();

// ============================================================================
// LAB 3
// ============================================================================
PrintHeader("LAB 3 - Persystencja i monitoring plików");

var repository   = new OrderRepository();
var reportBuilder = new XmlReportBuilder();
var dataPath     = Path.Combine(AppContext.BaseDirectory, "data");
var inboxPath    = Path.Combine(AppContext.BaseDirectory, "inbox");
var jsonPath     = Path.Combine(dataPath, "orders.json");
var xmlPath      = Path.Combine(dataPath, "orders.xml");
var reportPath   = Path.Combine(dataPath, "report.xml");

Console.WriteLine("\n--- Lab 3 / Zadanie 1: OrderRepository round-trip ---");
await repository.SaveToJsonAsync(orders, jsonPath);
await repository.SaveToXmlAsync(orders, xmlPath);
var fromJson = await repository.LoadFromJsonAsync(jsonPath);
var fromXml  = await repository.LoadFromXmlAsync(xmlPath);
Console.WriteLine($"JSON -> count={fromJson.Count}, total={fromJson.Sum(o => o.TotalAmount):0.00}");
Console.WriteLine($"XML  -> count={fromXml.Count}, total={fromXml.Sum(o => o.TotalAmount):0.00}");

RebindCustomers(fromJson, customers);
RebindCustomers(fromXml, customers);

Console.WriteLine("\n--- Lab 3 / Zadanie 2: XmlReportBuilder ---");
var xmlReport = reportBuilder.BuildReport(fromJson);
await reportBuilder.SaveReportAsync(xmlReport, reportPath);
Console.WriteLine($"Report saved: {reportPath}");
var highValueIds = await reportBuilder.FindHighValueOrderIdsAsync(reportPath, 1000m);
Console.WriteLine("Order IDs > 1000.00 from report:");
foreach (var id in highValueIds)
    Console.WriteLine($"  - {id}");

Console.WriteLine("\n--- Lab 3 / Zadanie 3: InboxWatcher ---");
var watcherPipeline = new OrderPipeline();
watcherPipeline.StatusChanged += (sender, args) =>
    Console.WriteLine($"[WATCHER-PIPELINE] {args.Timestamp:O} | Order {args.Order.Id} | {args.OldStatus} -> {args.NewStatus}");

using (var watcher = new InboxWatcher(inboxPath, watcherPipeline, maxConcurrentFiles: 2))
{
    watcher.Start();
    for (var i = 1; i <= 2; i++)
    {
        var batch    = BuildInboxOrders(products, customers, i);
        var filePath = Path.Combine(inboxPath, $"incoming-{DateTime.Now:yyyyMMdd-HHmmss}-{i}.json");
        await repository.SaveToJsonAsync(batch, filePath);
        Console.WriteLine($"[DEMO] Generated file: {filePath}");
        await Task.Delay(TimeSpan.FromSeconds(3));
    }
    Console.WriteLine("Watcher active for 5 more seconds...");
    await Task.Delay(TimeSpan.FromSeconds(5));
}

// ============================================================================
// LAB 4 — EF Core
// ============================================================================
PrintHeader("LAB 4 - Baza danych w OrderFlow (EF Core + SQLite)");

await using var db = new OrderFlowContext();
await db.Database.MigrateAsync();
await DatabaseSeeder.SeedAsync(db);

PrintHeader("LAB 4 / Zadanie 2 — CRUD");

Console.WriteLine("\n--- CREATE: Nowe zamówienie z 2 pozycjami ---");
var firstCustomer = await db.Customers.FirstAsync();
var firstProduct  = await db.Products.FirstAsync();
var secondProduct = await db.Products.Skip(1).FirstAsync();

var newOrder = new Order
{
    CustomerId = firstCustomer.Id,
    OrderDate  = DateTime.Now,
    Status     = OrderStatus.New,
    Notes      = "Zamówienie testowe z Lab 4",
    Items      =
    [
        new OrderItem { ProductId = firstProduct.Id,  Quantity = 1, UnitPrice = firstProduct.Price  },
        new OrderItem { ProductId = secondProduct.Id, Quantity = 2, UnitPrice = secondProduct.Price },
    ]
};
db.Orders.Add(newOrder);
await db.SaveChangesAsync();
Console.WriteLine($"Dodano zamówienie #{newOrder.Id} z 2 pozycjami dla klienta '{firstCustomer.FullName}'");

Console.WriteLine("\n--- READ: Zamówienia z Include ---");
var ordersFromDb = await db.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items).ThenInclude(i => i.Product)
    .OrderBy(o => o.Id)
    .ToListAsync();
foreach (var o in ordersFromDb)
{
    var total = o.Items.Sum(i => i.Quantity * i.UnitPrice);
    Console.WriteLine($"  #{o.Id} [{o.Status}] | {o.Customer.FullName} | {o.OrderDate:yyyy-MM-dd} | {total:F2} zł");
    foreach (var item in o.Items)
        Console.WriteLine($"       - {item.Product.Name} x{item.Quantity} @ {item.UnitPrice:F2} zł");
}

Console.WriteLine("\n--- UPDATE: Zmiana statusu New → Processing ---");
var orderToUpdate = await db.Orders.FirstAsync(o => o.Status == OrderStatus.New);
var oldStatus     = orderToUpdate.Status;
orderToUpdate.Status = OrderStatus.Processing;
orderToUpdate.Notes  = "Zaktualizowane przez Lab 4";
await db.SaveChangesAsync();
Console.WriteLine($"Zamówienie #{orderToUpdate.Id}: {oldStatus} → {orderToUpdate.Status}");

Console.WriteLine("\n--- DELETE: Usunięcie zamówienia Cancelled ---");
var cancelledOrder = await db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Status == OrderStatus.Cancelled);
if (cancelledOrder is not null)
{
    db.Orders.Remove(cancelledOrder);
    await db.SaveChangesAsync();
    Console.WriteLine($"Usunięto zamówienie #{cancelledOrder.Id}");
}
else
    Console.WriteLine("Brak zamówień o statusie Cancelled.");

Console.WriteLine("\n--- DELETE (Restrict): Próba usunięcia klienta z zamówieniami ---");
var customerWithOrders = await db.Customers.Include(c => c.Orders).FirstAsync(c => c.Orders.Any());
try
{
    db.Customers.Remove(customerWithOrders);
    await db.SaveChangesAsync();
}
catch (Exception ex)
{
    db.ChangeTracker.Clear();
    Console.WriteLine($"Wyjątek (oczekiwany): {ex.GetType().Name} → DeleteBehavior.Restrict działa.");
}

PrintHeader("LAB 4 / Zadanie 3 — Zapytania LINQ + transakcja");

Console.WriteLine("\n--- Q1: VIP orders > 1000 zł ---");
decimal threshold = 1000m;
var vipHighOrders = (await db.Orders.Include(o => o.Customer).Include(o => o.Items)
    .Where(o => o.Customer.IsVip).ToListAsync())
    .Where(o => o.Items.Sum(i => i.Quantity * i.UnitPrice) > threshold).ToList();
foreach (var o in vipHighOrders)
    Console.WriteLine($"  #{o.Id} | {o.Customer.FullName} [VIP] | {o.Items.Sum(i => i.Quantity * i.UnitPrice):F2} zł");
if (!vipHighOrders.Any()) Console.WriteLine("  (brak wyników)");

Console.WriteLine("\n--- Q2: Ranking klientów ---");
var customerRanking = await db.Customers
    .Select(c => new
    {
        c.FullName, c.IsVip,
        TotalValue = c.Orders.SelectMany(o => o.Items).Sum(i => (decimal?)i.Quantity * i.UnitPrice) ?? 0m,
        OrderCount = c.Orders.Count()
    })
    .OrderByDescending(x => x.TotalValue).ToListAsync();
foreach (var item in customerRanking)
    Console.WriteLine($"  {item.FullName}{(item.IsVip ? " [VIP]" : "")}: {item.OrderCount} zamówień, {item.TotalValue:F2} zł");

Console.WriteLine("\n--- Q3: Średnia per miasto ---");
var avgByCityList = await db.Orders.Include(o => o.Customer).Include(o => o.Items).ToListAsync();
var avgByCityResult = avgByCityList
    .GroupBy(o => o.Customer.City)
    .Select(g => new { City = g.Key, AvgValue = g.Average(o => o.Items.Sum(i => i.Quantity * i.UnitPrice)) })
    .OrderByDescending(x => x.AvgValue);
foreach (var item in avgByCityResult)
    Console.WriteLine($"  {item.City}: średnia {item.AvgValue:F2} zł");

Console.WriteLine("\n--- Q4: Produkty nigdy nie zamówione ---");
var neverOrdered = await db.Products.Where(p => !p.OrderItems.Any()).ToListAsync();
if (neverOrdered.Any())
    foreach (var p in neverOrdered)
        Console.WriteLine($"  {p.Name} — nigdy nie zamówiony");
else
    Console.WriteLine("  Wszystkie produkty były zamówione.");

Console.WriteLine("\n--- Q5: Dynamiczny filtr ---");
OrderStatus? filterStatus    = OrderStatus.Processing;
decimal      filterMinAmount = 500m;
IQueryable<Order> dynamicQuery = db.Orders.Include(o => o.Customer).Include(o => o.Items);
if (filterStatus.HasValue)
    dynamicQuery = dynamicQuery.Where(o => o.Status == filterStatus.Value);
var dynamicResult = (await dynamicQuery.ToListAsync())
    .Where(o => o.Items.Sum(i => i.Quantity * i.UnitPrice) >= filterMinAmount).ToList();
Console.WriteLine($"  Wyników: {dynamicResult.Count}");
foreach (var o in dynamicResult)
    Console.WriteLine($"  #{o.Id} | {o.Customer.FullName} | {o.Items.Sum(i => i.Quantity * i.UnitPrice):F2} zł");

PrintHeader("LAB 4 / Zadanie 3 — Transakcja");

var orderForSuccess = await db.Orders
    .Include(o => o.Items).ThenInclude(i => i.Product)
    .FirstOrDefaultAsync(o => o.Status == OrderStatus.New && o.Items.All(i => i.Product.StockQuantity >= i.Quantity));
if (orderForSuccess is not null)
{
    Console.WriteLine($"\n[SUKCES] Procesowanie zamówienia #{orderForSuccess.Id}...");
    await ProcessOrderAsync(db, orderForSuccess.Id);
}

Console.WriteLine("\n[BŁĄD] Scenariusz z niewystarczającym stociem...");
var productToZero = await db.Products.FirstAsync();
productToZero.StockQuantity = 0;
await db.SaveChangesAsync();

var failOrder = new Order
{
    CustomerId = firstCustomer.Id,
    OrderDate  = DateTime.Now,
    Status     = OrderStatus.New,
    Items      = [ new OrderItem { ProductId = productToZero.Id, Quantity = 1, UnitPrice = productToZero.Price } ]
};
db.Orders.Add(failOrder);
await db.SaveChangesAsync();

try { await ProcessOrderAsync(db, failOrder.Id); }
catch (InvalidOperationException ex)
{
    Console.WriteLine($"Wyjątek (oczekiwany): {ex.Message}");
    Console.WriteLine("→ Rollback wykonany.");
}

PrintHeader("LAB 4 — koniec demonstracji");

// ============================================================================
// LAB 5 — CurrencyService
// ============================================================================
PrintHeader("LAB 5 - Integracja z NBP API (CurrencyService)");

var httpClient      = new HttpClient();
var currencyService = new CurrencyService(httpClient);
var converter       = new OrderCurrencyConverter(currencyService);

await using var dbForCurrency = new OrderFlowContext();
await dbForCurrency.Database.MigrateAsync();

var ordersForCurrency = await dbForCurrency.Orders
    .Include(o => o.Customer)
    .Include(o => o.Items)
    .Take(3)
    .ToListAsync();

if (!ordersForCurrency.Any())
{
    Console.WriteLine("Brak zamówień w bazie.");
}
else
{
    Console.WriteLine($"\n{"#",-4} {"Klient",-20} {"PLN",10} {"USD",10} {"EUR",10}");
    Console.WriteLine(new string('-', 58));
    foreach (var o in ordersForCurrency)
    {
        var totalPln = o.Items.Sum(i => i.Quantity * i.UnitPrice);
        decimal? totalUsd = null;
        decimal? totalEur = null;
        try { totalUsd = await converter.ConvertOrderTotalAsync(o, "USD"); } catch { }
        try { totalEur = await converter.ConvertOrderTotalAsync(o, "EUR"); } catch { }
        Console.WriteLine(
            $"#{o.Id,-3} {o.Customer?.FullName ?? "?",-20} " +
            $"{totalPln,9:F2} " +
            $"{(totalUsd.HasValue ? totalUsd.Value.ToString("F2") : "błąd"),9} " +
            $"{(totalEur.HasValue ? totalEur.Value.ToString("F2") : "błąd"),9}");
    }
}

PrintHeader("OrderFlow — koniec wszystkich labów");

// ============================================================================
// FUNKCJE POMOCNICZE
// ============================================================================

void PrintHeader(string title)
{
    Console.WriteLine("\n" + new string('=', 70));
    Console.WriteLine(title.PadRight(70));
    Console.WriteLine(new string('=', 70));
}

static void RebindCustomers(List<Order> loadedOrders, List<Customer> customerCatalog)
{
    foreach (var order in loadedOrders)
        order.Customer = customerCatalog.FirstOrDefault(c => c.Id == order.CustomerId) ?? customerCatalog.First();
}

static List<Order> BuildInboxOrders(List<Product> products, List<Customer> customers, int batchNo)
{
    var baseOrderId = 10_000 + (batchNo * 100);
    var seedOrder   = SampleData.GetOrders(products, customers).First();
    var result      = new List<Order>();
    for (var i = 0; i < 3; i++)
    {
        var orderId  = baseOrderId + i;
        var customer = customers[(batchNo + i) % customers.Count];
        var product  = products[(batchNo + i) % products.Count];
        result.Add(new Order
        {
            Id         = orderId,
            CustomerId = customer.Id,
            Customer   = customer,
            OrderDate  = DateTime.Now,
            Status     = OrderStatus.New,
            Notes      = $"Inbox batch {batchNo}, order {i + 1}",
            Items      = new List<OrderItem>
            {
                new OrderItem
                {
                    Id        = seedOrder.Items.First().Id + orderId,
                    OrderId   = orderId,
                    ProductId = product.Id,
                    Product   = product,
                    Quantity  = i + 1,
                    UnitPrice = product.Price
                }
            }
        });
    }
    return result;
}

static async Task ProcessOrderAsync(OrderFlowContext db, int orderId)
{
    await using var transaction = await db.Database.BeginTransactionAsync();
    try
    {
        var order = await db.Orders
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new InvalidOperationException($"Zamówienie #{orderId} nie istnieje.");

        order.Status = OrderStatus.Processing;
        await db.SaveChangesAsync();
        Console.WriteLine($"  [{orderId}] Status: New → Processing");

        foreach (var item in order.Items)
        {
            if (item.Product.StockQuantity < item.Quantity)
                throw new InvalidOperationException(
                    $"Brak stanu dla '{item.Product.Name}': potrzeba {item.Quantity}, dostępne {item.Product.StockQuantity}.");
            item.Product.StockQuantity -= item.Quantity;
            Console.WriteLine($"  [{orderId}] Stock '{item.Product.Name}': -{item.Quantity} (pozostało: {item.Product.StockQuantity})");
        }

        order.Status = OrderStatus.Completed;
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        Console.WriteLine($"  [{orderId}] COMMIT OK → Completed");
    }
    catch
    {
        await transaction.RollbackAsync();
        db.ChangeTracker.Clear();
        Console.WriteLine($"  [{orderId}] ROLLBACK");
        throw;
    }
}