using System.Globalization;
using Core.Dto;
namespace Core.Import;
public static class ProductCsvImporter
{
    private const char Separator = ';';

    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовка
            if (number == 1 && (line.StartsWith("type", StringComparison.OrdinalIgnoreCase) || line.StartsWith("id", StringComparison.OrdinalIgnoreCase)))
                continue;
            switch (ParseLine(line))
            {
                case ProductOutcome productOk:
                    products.Add(productOk.Product);
                    break;
                case WarehouseOutcome warehouseOk:
                    warehouses.Add(warehouseOk.Warehouse);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);
        return parts switch
        {
            // --- 1. РОЗБІР СКЛАДІВ (Префікс "W") ---
            ["W", ..] when parts.Length < 4 => new ParseFailed($"для складу очікую 4 колонки, отримано {parts.Length}"),
            ["W", "", _, _] or ["W", _, "", _] or ["W", _, _, ""] => new ParseFailed("у записі складу є порожні поля"),
            ["W", var id, var name, var loc] => new WarehouseOutcome(new WarehouseDto(id, name, loc)),
            ["W", ..] => new ParseFailed($"занадто багато колонок для складу: {parts.Length}"),

            // --- 2. РОЗБІР ТОВАРІВ (Префікс "P") ---
            ["P", ..] when parts.Length < 6 => new ParseFailed($"для товару очікую щонайменше 6 колонок (із префіксом), отримано {parts.Length}"),
            ["P", _, "", _, _, _, ..] or ["P", _, _, "", _, _, ..] => new ParseFailed("SKU або назва товару порожні"),
            ["P", _, _, _, _, var qty] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість товару '{qty}' не є невід'ємним цілим числом"),
            ["P", _, _, _, _, var qty, _] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість товару '{qty}' не є невід'ємним цілим числом"),

            // Успішний товар без примітки (6 стовпців разом із 'P')
            ["P", var id, var sku, var name, var unit, var qty]
                => new ProductOutcome(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture))),

            // Успішний товар із приміткою (7 стовпців разом із 'P')
            ["P", var id, var sku, var name, var unit, var qty, var note]
                => new ProductOutcome(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(note) ? null : note)),

            ["P", ..] => new ParseFailed($"занадто багато колонок для товару: {parts.Length}"),

            // --- 3. НЕВІДОМИЙ ТИП ЗАПИСУ ---
            [var prefix, ..] => new ParseFailed($"невідомий тип запису '{prefix}' (очікується 'P' або 'W')"),
            _ => new ParseFailed("порожній або некоректний рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ProductOutcome(ProductDto Product) : ParseOutcome;
    private sealed record WarehouseOutcome(WarehouseDto Warehouse) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}