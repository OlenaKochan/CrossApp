using Core.Dto;
using Core.Import;
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

string extension = Path.GetExtension(path).ToLowerInvariant();

// 1. Вибір імпортера за розширенням файлу через switch expression
MixedImportResult? result = extension switch
{
    ".csv" => ProductCsvImporter.Load(path),
    ".json" => ProductJsonImporter.Load(path),
    _ => null
};

if (result is null)
{
    Console.WriteLine($"Непідтримуваний формат файлу: {extension}. Підтримуються тільки .csv та .json.");
    return 1;
}
Console.WriteLine($"Файл: {Path.GetFileName(path)}");
Console.WriteLine($"Завантажено товарів: {result.Products.Count}, складів: {result.Warehouses.Count}");

if (result.Warehouses.Count > 0)
{
    Console.WriteLine("\nСклади:");
    foreach (WarehouseDto w in result.Warehouses)
    {
        Console.WriteLine($"  {w.Id,-6} {w.Name,-28} {w.Location}");
    }
}

if (result.Products.Count > 0)
{
    Console.WriteLine("\nТовари (перші 5):");
    Console.WriteLine($"  {"ID",-7} {"SKU",-9} {"Назва",-34} {"К-сть",5} {"Од.",-5} {"Примітка"}");
    Console.WriteLine("  " + new string('-', 76));

    foreach (ProductDto p in result.Products.Take(5))
    {
        string noteText = p.Note ?? "-";
        Console.WriteLine($"  {p.Id,-7} {p.Sku,-9} {p.Name,-34} {p.Quantity,5} {p.Unit,-5} {noteText}");
    }
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків / помилок: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}

// 3. Статистика імпорту одним рядком
int accepted = result.Products.Count + result.Warehouses.Count;
int rejected = result.Errors.Count;
int total = accepted + rejected;
double errorPercent = total > 0 ? (double)rejected / total * 100 : 0.0;

Console.WriteLine($"\n[СТАТИСТИКА] Усього рядків: {total} | Прийнято: {accepted} | Пропущено: {rejected} | Помилок: {errorPercent:F1}%");

return 0;