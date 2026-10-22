using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Reports;
using Core.Services;
using Core.Storage;

// 1. Ініціалізація сховища (ручний DI з ЛР5)
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);
Console.WriteLine($"Сховище: {store.GetType().Name}\n");

var items = service.All();

// 2. Визначення підкоманди звіту (ігноруючи прапорці на зразок --file)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;

    case "report-low":
        Print(ReportService.LowStock(items, 10));
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

    case "demo-deferred":
        RunDeferredExecutionDemo();
        break;

    default:
        Console.WriteLine("Доступні команди: report-top | report-low | report-groups | report-sum | demo-deferred");
        break;
}

return 0;

static void Print(IReadOnlyList<StockLine> lines)
{
    foreach (var l in lines)
    {
        Console.WriteLine($"{l.Sku,-10} | {l.Name,-22} | {l.Quantity,6} | {l.Unit}");
    }
}

static void RunDeferredExecutionDemo()
{
    Console.WriteLine("— Демонстрація відкладеного виконання LINQ —");
    var source = SampleData.Products().ToList();
    var query = source.Where(p => p.Quantity < 10); // Запит побудовано, але не виконано

    Console.WriteLine($"Дефіцитних до змін: {query.Count()}");
    
    // Змінюємо джерело після побудови запиту
    source.Add(Product.Create("tmp00001", "SKU-999", "Скоба ремонтна", "шт", 3));
    
    Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // Число інше

    var snapshot = query.ToList(); // Матеріалізація запиту
    source.Clear();
    
    Console.WriteLine($"У знімку лишилось: {snapshot.Count}"); // Не змінюється
}