using System.Globalization;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    public static MixedImportResult Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path);

        using var doc = JsonDocument.Parse(json);
        JsonElement root = doc.RootElement;

        // 1. Розбір складів
        if (root.TryGetProperty("warehouses", out JsonElement warehousesElem) && warehousesElem.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement w in warehousesElem.EnumerateArray())
            {
                string id = w.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                string name = w.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                string loc = w.TryGetProperty("location", out var locProp) ? locProp.GetString() ?? "" : "";

                if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(loc))
                {
                    errors.Add($"JSON склад: одне або декілька полів є порожніми");
                }
                else
                {
                    warehouses.Add(new WarehouseDto(id, name, loc));
                }
            }
        }

        // 2. Розбір товарів (поелементно через switch expression)
        if (root.TryGetProperty("products", out JsonElement productsElem) && productsElem.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (JsonElement p in productsElem.EnumerateArray())
            {
                index++;

                string id = p.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "" : "";
                string sku = p.TryGetProperty("sku", out var skuProp) ? skuProp.GetString() ?? "" : "";
                string name = p.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? "" : "";
                string unit = p.TryGetProperty("unit", out var unitProp) ? unitProp.GetString() ?? "" : "";
                string? note = p.TryGetProperty("note", out var noteProp) ? noteProp.GetString() : null;

                // беремо RawText з JSON (наприклад: "45", "45.5", "-10", "багато")
                string rawQty = p.TryGetProperty("quantity", out var qProp) ? qProp.GetRawText().Trim('\"') : "";

                switch (ParseProduct(id, sku, name, unit, rawQty, note))
                {
                    case ParseOk ok:
                        products.Add(ok.Product);
                        break;
                    case ParseFailed failed:
                        errors.Add($"JSON товар #{index}: {failed.Reason}");
                        break;
                }
            }
        }

        return new MixedImportResult(products, warehouses, errors);
    }

    //switch expression з патернами
    private static ParseOutcome ParseProduct(string id, string sku, string name, string unit, string rawQty, string? note)
    {
        return (sku, name, rawQty) switch
        {
            // 1. Порожні обов'язкові поля
            ("", _, _) or (_, "", _)
                => new ParseFailed(" (SKU) або назва порожні"),

            // 2. Дробове, текстове або від'ємне число 
            _ when !int.TryParse(rawQty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0
                => new ParseFailed($"кількість '{rawQty}' не є невід'ємним цілим числом"),

            // 3. Успішний результат
            _ => new ParseOk(new ProductDto(id, sku, name, unit, int.Parse(rawQty, CultureInfo.InvariantCulture), string.IsNullOrWhiteSpace(note) ? null : note))
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(ProductDto Product) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}