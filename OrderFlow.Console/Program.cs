using OrderFlow.Console.Models;
using OrderFlow.Console.Services;

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

// Test walidacji zamówienia poprawnego
Console.WriteLine("\n--- Walidacja zamówienia POPRAWNEGO (ID=1) ---");
var validOrder = orders.First(o => o.Id == 1);
var result = validator.ValidateAll(validOrder);
Console.WriteLine(result);

// Test walidacji zamówienia łamiącego kilka reguł
Console.WriteLine("\n--- Walidacja zamówienia BŁĘDNEGO (ID=6 - z przyszłości) ---");
var invalidOrder = orders.First(o => o.Id == 6);
result = validator.ValidateAll(invalidOrder);
Console.WriteLine(result);

// ============================================================================
// ZADANIE 3 - Action, Func, Predicate
// ============================================================================
PrintHeader("ZADANIE 3 - Action, Func, Predicate");

var processor = new OrderProcessor();

// 1. PREDICATE - Filtrowanie zamówień
Console.WriteLine("\n--- Filtry Predicate (3 różne) ---");

var completedOrders = processor.FilterOrders(orders, OrderProcessorExtensions.ByStatus(OrderStatus.Completed));
Console.WriteLine($"\n1. Zamówienia skompletowane ({completedOrders.Count}):");
completedOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.Name} - {o.TotalAmount}zł"));

var vipOrders = processor.FilterOrders(orders, OrderProcessorExtensions.FromVipCustomers());
Console.WriteLine($"\n2. Zamówienia od VIP klientów ({vipOrders.Count}):");
vipOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.Name} - {o.TotalAmount}zł"));

var largeOrders = processor.FilterOrders(orders, OrderProcessorExtensions.LargeOrders());
Console.WriteLine($"\n3. Duże zamówienia (>5000zł) ({largeOrders.Count}):");
largeOrders.ForEach(o => Console.WriteLine($"  - #{o.Id}: {o.Customer?.Name} - {o.TotalAmount}zł"));

// 2. ACTION - Wykonywanie akcji
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

// 3. FUNC - Projekcja zamówień
Console.WriteLine("\n--- Projekcja Func na typ anonimowy ---");
var summaries = processor.ProjectOrders(orders.Take(3).ToList(), OrderProcessorExtensions.ProjectToOrderSummary());
foreach (var summary in summaries)
{
    Console.WriteLine($"  {summary}");
}

// 4. AGREGACJA - Func<IEnumerable<Order>, decimal>
Console.WriteLine("\n--- Agregacja (3 agregatory) ---");
Console.WriteLine($"1. Suma kwot: {processor.Aggregate(orders, OrderProcessorExtensions.SumTotal()):C}");
Console.WriteLine($"2. Średnia kwota: {processor.Aggregate(orders, OrderProcessorExtensions.AverageTotal()):C}");
Console.WriteLine($"3. Maksymalna kwota: {processor.Aggregate(orders, OrderProcessorExtensions.MaxTotal()):C}");

// 5. ŁAŃCUCH - Filtruj → Sortuj → Top N → Wypisz
Console.WriteLine("\n--- Łańcuch operacji: filtruj → sortuj → top 2 → wypisz ---");
Console.WriteLine("2 największe zamówienia od klientów (po filtracji):");
processor.FilterSortAndExecute(
    orders,
    o => o.TotalAmount > 1000m,
    o => o.TotalAmount,
    2,
    o => Console.WriteLine($"  #{o.Id}: {o.Customer?.Name} - {o.TotalAmount}zł")
);

// Przywracamy oryginalne statusy
testOrders[0].Status = OrderStatus.Validated;
testOrders[1].Status = OrderStatus.New;

// ============================================================================
// ZADANIE 4 - LINQ (method syntax i query syntax)
// ============================================================================
PrintHeader("ZADANIE 4 - LINQ");

// ZAPYTANIE 1: GroupBy z agregacją - Top klienci wg kwoty
Console.WriteLine("\n--- Q1: GroupBy + agregacja (Method Syntax) ---");
Console.WriteLine("Top klienci wg całkowitej kwoty zamówień:");
var q1 = orders
    .GroupBy(o => o.Customer)
    .Select(g => new
    {
        Customer = g.Key.Name,
        OrderCount = g.Count(),
        TotalAmount = g.Sum(o => o.TotalAmount),
        AverageOrder = g.Average(o => o.TotalAmount)
    })
    .OrderByDescending(x => x.TotalAmount)
    .Take(3);
foreach (var item in q1)
    Console.WriteLine($"  {item.Customer}: {item.OrderCount} zamówień, razem {item.TotalAmount}zł (śr. {item.AverageOrder:F0}zł)");

// ZAPYTANIE 2: Join - Zamówienia z klientami, grupowanie po mieście
Console.WriteLine("\n--- Q2: Join (Query Syntax) ---");
Console.WriteLine("Zamówienia zgrupowane po mieście klienta:");
var q2 = from order in orders
         join customer in customers on order.CustomerId equals customer.Id
         group new { order, customer } by customer.City into cityGroup
         select new
         {
             City = cityGroup.Key,
             OrderCount = cityGroup.Count(),
             TotalAmount = cityGroup.Sum(x => x.order.TotalAmount)
         };
foreach (var item in q2.OrderByDescending(x => x.TotalAmount))
    Console.WriteLine($"  {item.City}: {item.OrderCount} zamówień, {item.TotalAmount}zł");

// ZAPYTANIE 3: SelectMany (spłaszczenie) - Order → OrderItems → Products
Console.WriteLine("\n--- Q3: SelectMany (Method Syntax) ---");
Console.WriteLine("Produkty We wszystkich zamówieniach:");
var q3 = orders
    .SelectMany(o => o.Items)
    .Select(i => i.Product)
    .Distinct()
    .OrderBy(p => p.Name);
foreach (var product in q3)
    Console.WriteLine($"  {product.Name} ({product.Category})");

// ZAPYTANIE 4: GroupBy agregacja - Średnia wartość per kategoria
Console.WriteLine("\n--- Q4: GroupBy (Query Syntax) ---");
Console.WriteLine("Średnia wartość produktu per kategoria:");
var q4 = from product in products
         group product by product.Category into catGroup
         select new
         {
             Category = catGroup.Key,
             ProductCount = catGroup.Count(),
             AvgPrice = catGroup.Average(p => p.Price)
         };
foreach (var item in q4.OrderByDescending(x => x.AvgPrice))
    Console.WriteLine($"  {item.Category}: {item.ProductCount} produktów, śr. cena {item.AvgPrice:F2}zł");

// ZAPYTANIE 5: GroupJoin (Left Join pattern)
Console.WriteLine("\n--- Q5: GroupJoin (Method Syntax - Left Join Pattern) ---");
Console.WriteLine("Wszyscy klienci i ich zamówienia:");
var q5 = customers
    .GroupJoin(
        orders,
        customer => customer.Id,
        order => order.CustomerId,
        (customer, customerOrders) => new
        {
            CustomerName = customer.Name,
            IsVip = customer.IsVip,
            OrderCount = customerOrders.Count(),
            TotalSpent = customerOrders.Sum(o => o.TotalAmount)
        }
    )
    .OrderByDescending(x => x.TotalSpent);
foreach (var item in q5)
    Console.WriteLine($"  {item.CustomerName} {(item.IsVip ? "[VIP]" : "")}: " +
        $"{item.OrderCount} zamówień, razem {item.TotalSpent:F0}zł");

// ZAPYTANIE 6: Mieszana składnia - Raport per klient z ulubioną kategorią
Console.WriteLine("\n--- Q6: Mixed Syntax - Raport per klient z ulubioną kategorią ---");
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
             Customer = customer.Name,
             OrderCount = customerOrders.Count(),
             TotalAmount = customerOrders.Sum(o => o.TotalAmount),
             FavoriteCategory = favoriteCategory
         }
         into report
         orderby report.TotalAmount descending
         select report;

foreach (var item in q6)
    Console.WriteLine($"  {item.Customer}: {item.OrderCount} zamówień, {item.TotalAmount}zł, " +
        $"ulub. kategoria: {item.FavoriteCategory}");

Console.WriteLine();
PrintHeader("Projekt OrderFlow - koniec demonstracji");

// ============================================================================
// FUNKCJE POMOCNICZE
// ============================================================================

void PrintHeader(string title)
{
    Console.WriteLine("\n" + new string('=', 70));
    Console.WriteLine(title.PadRight(70));
    Console.WriteLine(new string('=', 70));
}
