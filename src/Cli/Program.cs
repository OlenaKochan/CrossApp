using Core.Dto;
using Core.Import;
string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}

ImportResult<ProductDto> result = ProductCsvImporter.Load(path);

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
Console.WriteLine($"{"ID",-7} {"SKU",-9} {"Назва",-34} {"К-сть",5} {"Од.",-5} {"Примітка"}");
Console.WriteLine(new string('-', 75));

foreach (ProductDto p in result.Items.Take(5))
{
    string noteText = p.Note ?? "-";
    Console.WriteLine($"{p.Id,-7} {p.Sku,-9} {p.Name,-34} {p.Quantity,5} {p.Unit,-5} {noteText}");
}

if (result.Errors.Count > 0)
{
    Console.WriteLine($"\nПропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
    {
        Console.WriteLine($"  ! {e}");
    }
}
return 0;