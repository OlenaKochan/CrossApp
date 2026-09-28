using System.Globalization;
using Core.Dto;
namespace Core.Import;
public static class ProductCsvImporter
{
    private const char Separator = ';';
    public static ImportResult<ProductDto> Load(string path)
    {
        var items = new List<ProductDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            // Пропуск рядка заголовків
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;
            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<ProductDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // Менше 5 колонок — помилка структури
            { Length: < 5 } => new ParseFailed($"очікую щонайменше 5 колонок, отримано {parts.Length}"),

            // Порожні артикул (SKU) або назва (працює і для 5, і для 6 колонок завдяки зрізу '..')
            [_, "", _, _, _, ..] or [_, _, "", _, _, ..]
                => new ParseFailed("SKU або назва порожні"),

            // Некоректне або від'ємне число кількості (для 5 колонок)
            [_, _, _, _, var qty] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість '{qty}' не є невід'ємним цілим числом"),

            // Некоректне або від'ємне число кількості (для 6 колонок, де 6-й елемент — примітка)
            [_, _, _, _, var qty, _] when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість '{qty}' не є невід'ємним цілим числом"),

            // Розбір 5 колонок (без примітки)
            [var id, var sku, var name, var unit, var qty]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture))),

            // Розбір 6 колонок (із приміткою Note)
            [var id, var sku, var name, var unit, var qty, var note]
                => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(qty, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(note) ? null : note)),

            // Більше 6 колонок
            _ => new ParseFailed($"занадто багато колонок: {parts.Length}")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}