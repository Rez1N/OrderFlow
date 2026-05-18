# OrderFlow - System Przetwarzania Zamówień

Aplikacja demonstracyjna zbudowana w .NET 6.0, implementująca kompleksowy system zarządzania zamówieniami z wykorzystaniem zaawansowanych funkcji C#.

## 📋 Struktura Projektu

```
OrderFlow/
├── OrderFlow.sln
└── OrderFlow.Console/
    ├── Models/
    │   ├── OrderStatus.cs      # Enum statusów zamoówienia
    │   ├── Product.cs          # Model produktu
    │   ├── Customer.cs         # Model klienta
    │   ├── Order.cs            # Model zamówienia z TotalAmount
    │   └── OrderItem.cs        # Model pozycji zamówienia z TotalPrice
    ├── Services/
    │   ├── SampleData.cs       # Statyczne dane testowe (5 produktów, 4 klientów, 6 zamówień)
    │   ├── OrderValidator.cs   # Walidacja delegatami + Func<>
    │   └── OrderProcessor.cs   # Przetwarzanie zamówień (Action, Func, Predicate)
    ├── Program.cs              # Demonstracja wszystkich funkcjonalności + LINQ
    └── OrderFlow.Console.csproj
```

## ✅ Realizowane Zadania

### Zadanie 1: Model Domenowy i Dane Testowe (3 pkt)
- ✓ Klasy: `Product`, `Customer`, `Order`, `OrderItem`
- ✓ Właściwości obliczane: `Order.TotalAmount`, `OrderItem.TotalPrice`
- ✓ Enum: `OrderStatus` (New, Validated, Processing, Completed, Cancelled)
- ✓ `SampleData` z 5 produktami, 4 klientami (1 VIP), 6 zamówieniami

### Zadanie 2: Delegaty i Walidacja (4 pkt)
- ✓ Custom delegat `ValidationRule` z `out string errorMessage`
- ✓ 3+ reguły as named methods (pozycje, limit kwoty, ilości > 0)
- ✓ Walidacja oparty na `Func<Order, bool>` (data nie z przyszłości, status != Cancelled)
- ✓ Metoda `ValidateAll()` łącząca oba mechanizmy
- ✓ Demonstracja walidacji zamówienia poprawnego i błędnego

### Zadanie 3: Action, Func, Predicate (4 pkt)
- ✓ 3 różne `Predicate<Order>` (status, VIP, duże kwoty)
- ✓ 2+ zastosowania `Action<Order>` (wypisanie, zmiana statusu)
- ✓ Generyczna `Func<Order, T>` z projekcją na typ anonimowy
- ✓ Agregacja `Func<IEnumerable<Order>, decimal>` (suma, średnia, max)
- ✓ Łańcuch: filtruj → sortuj → top N → wypisz

### Zadanie 4: LINQ (4 pkt)
- ✓ 6+ zapytań w obu składniach (method + query syntax)
- ✓ **Q1**: GroupBy z Sum/Average (top klienci)
- ✓ **Q2**: Join z query syntax (zamówienia po mieście)
- ✓ **Q3**: SelectMany (spłaszczenie Order → Items → Products)
- ✓ **Q4**: GroupBy agregacja per kategoria
- ✓ **Q5**: GroupJoin (left join pattern - wszyscy klienci i ich zamówienia)
- ✓ **Q6**: Mixed syntax z `let` (raport with ulubiona kategoria)

## 🚀 Uruchomienie

### Budowanie

```bash
cd OrderFlow/OrderFlow.Console
dotnet build
```

### Uruchomienie

```bash
dotnet run
```

## 📚 Kluczowe Koncepty

### Delegaty i Func
- Custom delegat `ValidationRule` dla elastycznego systemu walidacji
- `Predicate<T>` do filtrowania
- `Action<T>` do wykonywania operacji
- `Func<T, R>` do transformacji danych

### LINQ
- **Method Syntax**: proceduralne, fluent API
- **Query Syntax**: deklaratywne, podobne do SQL
- **Mixed Syntax**: kombinacja obu dla maksymalnej czytelności
- Zaawansowane operatory: `GroupBy`, `GroupJoin`, `SelectMany`, `Join`

### Architektura
- Rozdzielenie odpowiedzialności (Models, Services)
- Dane testowe w osobnej klasie (`SampleData`)
- Rozszerzalny system walidacji
- Generyczne delegaty dla elastyczności

## 📊 Format Output

Program wyświetla:

1. **Zadanie 1**: Listy produktów, klientów i zamówień
2. **Zadanie 2**: Wyniki walidacji (pozytywna i negatywna)
3. **Zadanie 3**: Filtry, akcje, projekcje i agregacje
4. **Zadanie 4**: Wyniki 6 zapytań LINQ w obu składniach

Wszystkie dane są formatowane do łatwy czytania.

## 🎯 Technologie

- **.NET 6.0** (LTS - Long-Term Support)
- **C# 10.0** (nullable reference types, top-level statements)
- **LINQ** (Language Integrated Query)

## 📝 Notatki

- Ostrzeżenia CS8618 (nullable properties) są bezpieczne - właściwości zawsze są ustawiane w getData
- Projekt demonstruje praktyczne zastosowania zaawansowanych funkcji C#
- Kod jest skomentowany i gotowy do rozbudowy o zdarzenia, asynchroniczność i bazę danych
