using Core.Domain;
using Core.Dto;

// ОСНОВНА ЧАСТИНА 
Console.WriteLine("— Сценарій 1: успіх —");
Product product = Product.Create("G-001", "sku-g01", "Секатор садовий Pro", "шт", 100);
Console.WriteLine(product);
product.RegisterArrival(50);
product.Issue(30);
Console.WriteLine(product);

Product product2 = Product.Create("G-004", "sku-g04", "Шланг поливальний 25м", "бухт", 20);
Console.WriteLine(product2);
product2.RegisterArrival(10);
product2.Issue(5);
Console.WriteLine(product2);
Console.WriteLine();

Console.WriteLine("— Сценарій 2: порушення інваріантів —");
TryDo("видача більша за залишок", () => product.Issue(1000));
TryDo("порожній SKU", () => Product.Create("G-002", "", "Пісок", "т", 10));
TryDo("від'ємний залишок", () => Product.Create("G-003", "SKU-G03", "Цегла", "шт", -5));
TryDo("порожній ID", () => Product.Create("", "SKU-G05", "Лопата", "шт", 10));
TryDo("порожня назва", () => Product.Create("G-006", "SKU-G06", "", "шт", 15));
Console.WriteLine();

// ДОДАТКОВІ ЗАВДАННЯ 
Console.WriteLine("— Додаткове завдання 1: перевірка інваріантів для ImportResult —");
var dtos = new List<ProductDto>
{
    new("G-010", "SKU-G10", "Лопата садова", "шт", 20),
    new("G-011", "", "Саджанець груші", "шт", 5),
    new("G-012", "SKU-G12", "Шпагат", "м", -10),
    new("G-013", "SKU-G13", "Ґрунт універсальний", "міш", 40),
    new("G-014", "SKU-G14", "", "шт", 15),
    new("", "SKU-G15", "Горщик керамічний", "шт", 8)
};
var importResult = new ImportResult<ProductDto>(dtos, []);
var domainResult = ProductDomainLoader.FromImportResult(importResult);

Console.WriteLine($"Успішно відновлено сутностей: {domainResult.Entities.Count}");
foreach (var p in domainResult.Entities)
    Console.WriteLine($"  {p}");

Console.WriteLine($"Помилки доменних інваріантів: {domainResult.DomainErrors.Count}");
foreach (var err in domainResult.DomainErrors)
    Console.WriteLine($"  ! {err}");
Console.WriteLine();

Console.WriteLine("— Додаткове завдання 2: інваріант для двох сутностей —");
var warehouse = Warehouse.Create("W-01", "Центральний склад", maxCapacity: 1);
var p1 = Product.Create("P-01", "SKU-01", "Яблуня", "шт", 10);
var p2 = Product.Create("P-02", "SKU-02", "Груша", "шт", 5);
var pZero = Product.Create("P-03", "SKU-03", "Порожній ящик", "шт", 0);

StockPlacementService.PlaceProductToWarehouse(p1, warehouse);
Console.WriteLine($"Товар {p1.Sku} розміщено на складі {warehouse.Name}");
TryDo("перевищення місткості складу", () => StockPlacementService.PlaceProductToWarehouse(p2, warehouse));
TryDo("розміщення товару з нульовим залишком", () => StockPlacementService.PlaceProductToWarehouse(pZero, warehouse));
TryDo("створення складу з невалідною місткістю", () => Warehouse.Create("W-02", "Склад добрив", 0));
Console.WriteLine();

Console.WriteLine("— Додаткове завдання 3: переходи станів накладної —");
var order = ShipmentOrder.Create("SH-001");
Console.WriteLine(order);
order.ChangeStatus(ShipmentStatus.Confirmed);
Console.WriteLine(order);
TryDo("повернення з Confirmed у Draft", () => order.ChangeStatus(ShipmentStatus.Draft));
order.ChangeStatus(ShipmentStatus.Cancelled);
Console.WriteLine(order);
TryDo("зміна після Cancelled", () => order.ChangeStatus(ShipmentStatus.Confirmed));

var order2 = ShipmentOrder.Create("SH-002");
Console.WriteLine(order2);
order2.ChangeStatus(ShipmentStatus.Cancelled);
Console.WriteLine(order2);
TryDo("повернення зі скасованого в чернетку", () => order2.ChangeStatus(ShipmentStatus.Draft));
TryDo("повторне встановлення того самого стану", () => order2.ChangeStatus(ShipmentStatus.Cancelled));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював — інваріант відсутній!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}