using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Reports;
using Core.Services;
using Core.Storage;

// ==========================================
// БЛОК 1: Ініціалізація сховища та сервісу (ЛР 5)
// ==========================================
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);
var items = service.All();

// Визначаємо команду звітів з аргументів (ігноруючи прапорці на зразок --file)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

// ==========================================
// БЛОК 2: Обробка команд та вивід звітів (ЛР 7)
// ==========================================
switch (command)
{
    // Основні звіти
    case "report-top":
        PrintLines(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        PrintLines(ReportService.LowStock(items, 10));
        break;

    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
        {
            Console.WriteLine($"{g.Key,-6} | позицій: {g.Count,3} | сумарно: {g.TotalQuantity,6}");
        }
        break;

    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($"Усього позицій: {t.Items}, всього од.: {t.Total}, середнє: {t.Average:F2}, макс: {t.Max}");
        break;

    // Демонстрація відкладеного виконання
    case "demo-deferred":
        RunDeferredExecutionDemo();
        break;

    // --- БЛОК ДОДАТКОВИХ ЗАВДАНЬ ---
    case "bonus-join":
        RunJoinDemo(items);
        break;

    case "bonus-query-syntax":
        RunQuerySyntaxDemo(items);
        break;

    case "bonus-benchmark":
        RunPerformanceBenchmark();
        break;

    default:
        Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum | demo-deferred | bonus-join | bonus-query-syntax | bonus-benchmark");
        break;
}

return 0;


// ==========================================
// БЛОК 3: Допоміжні методи та демонстрації
// ==========================================

static void PrintLines(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.Sku,-10} | {l.Name,-22} | {l.Quantity,6} | {l.Unit}");
    }
}

// Демонстрація відкладеного виконання (Deferred Execution)
static void RunDeferredExecutionDemo()
{
    Console.WriteLine("— Демонстрація відкладеного виконання LINQ —");
    var source = SampleData.Products().ToList();
    var query = source.Where(p => p.Quantity < 10); // Запит побудовано, але не виконано

    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    source.Add(Product.Create("tmp00001", "SKU-999", "Скоба ремонтна", "шт", 3));
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}");

    var snapshot = query.ToList(); // Матеріалізація в знімок
    source.Clear();
    Console.WriteLine($"У знімку лишилось: {snapshot.Count}");
}

// Демонстрація Додаткового завдання 1 (Join та GroupJoin)
static void RunJoinDemo(IReadOnlyList<Product> items)
{
    Console.WriteLine("— Додаткове завдання 1: Join та GroupJoin —");
    if (items.Count == 0) return;

    List<StockMovement> movements =
    [
        new(items[0].Id, 10, "Прихід"),
        new(items[0].Id, -5, "Видача"),
        new(items[1].Id, 20, "Прихід")
    ];

    Console.WriteLine("Результат звичайного Join:");
    var joinResult = ReportService.GetProductMovements(items, movements);
    foreach (var m in joinResult)
    {
        Console.WriteLine($"  SKU: {m.Sku} | Товар: {m.Name,-20} | Дельта: {m.Delta:+0;-0} | Тип: {m.OperationType}");
    }

    Console.WriteLine("\nРезультат GroupJoin:");
    var groupJoinResult = ReportService.GetProductsWithGroupedMovements(items, movements);
    foreach (var g in groupJoinResult.Take(5))
    {
        Console.WriteLine($"  SKU: {g.Sku} | Товар: {g.Name,-20} | Кількість операцій: {g.MoveCount} | Загальна дельта: {g.TotalDelta}");
    }
}

// Демонстрація Додаткового завдання 3 (Query Syntax)
static void RunQuerySyntaxDemo(IReadOnlyList<Product> items)
{
    Console.WriteLine("— Додаткове завдання 3: Grouping через Query Syntax —");
    var result = ReportService.ByUnitQuerySyntax(items);
    foreach (var g in result)
    {
        Console.WriteLine($"  Одиниця: {g.Key,-6} | Позицій: {g.Count,3} | Сума залишків: {g.TotalQuantity,6}");
    }
}

// Демонстрація Додаткового завдання 4 (Stopwatch бенчмарк)
static void RunPerformanceBenchmark()
{
    Console.WriteLine("— Додаткове завдання 4: Benchmark (Stopwatch на 100 000 записах) —");
    var bigList = Enumerable.Range(1, 100000)
        .Select(i => Product.Create($"id_{i}", $"SKU-{i}", $"Товар {i}", i % 2 == 0 ? "шт" : "м", i % 50))
        .ToList();

    var sw = System.Diagnostics.Stopwatch.StartNew();
    var r1 = bigList.Where(p => p.Quantity > 10).ToList().OrderBy(p => p.Quantity).Take(5).ToList();
    sw.Stop();
    Console.WriteLine($"  З проміжним ToList(): {sw.ElapsedMilliseconds} ms");

    sw.Restart();
    var r2 = bigList.Where(p => p.Quantity > 10).OrderBy(p => p.Quantity).Take(5).ToList();
    sw.Stop();
    Console.WriteLine($"  Без проміжного ToList(): {sw.ElapsedMilliseconds} ms");
}