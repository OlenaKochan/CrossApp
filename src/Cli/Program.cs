using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Services;
using Core.Storage;

// Composition Root: єдине місце вибору конкретних класів[cite: 16, 19]
bool useFile = args.Contains("--file");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "catalog.json");

ICatalogStore store = useFile
    ? new FileCatalogStore(dataPath)
    : new InMemoryCatalogStore(SampleData.Products());

var service = new CatalogService(store);

Console.WriteLine($"Сховище: {store.GetType().Name}");
if (useFile)
{
    Console.WriteLine($"Шлях до файлу: {dataPath}");
}
Console.WriteLine();

// 1. ДЕМОНСТРАЦІЯ БІЗНЕС-ОПЕРАЦІЙ СЕРВІСУ (Add, Receive, Issue, Find, Delete)
Console.WriteLine("— 1. Операції сервісу: додавання товарів (Add) —");
var item1 = service.Add("SKU-901", "Капельна стрічка 100м", "бухт", 10, "Новинка");
Console.WriteLine($"  [Додано]   {item1.Id} [{item1.Sku}] {item1.Name,-26} — {item1.Quantity,3} {item1.Unit}");

var item2 = service.Add("SKU-902", "Саджанець Магнолії", "шт", 5, "Декоративні");
Console.WriteLine($"  [Додано]   {item2.Id} [{item2.Sku}] {item2.Name,-26} — {item2.Quantity,3} {item2.Unit}");

var item3 = service.Add("SKU-903", "Система туманоутворення", "компл", 8);
Console.WriteLine($"  [Додано]   {item3.Id} [{item3.Sku}] {item3.Name,-26} — {item3.Quantity,3} {item3.Unit}");

var item4 = service.Add("SKU-904", "Горщик торф'яний 80мм", "уп", 50);
Console.WriteLine($"  [Додано]   {item4.Id} [{item4.Sku}] {item4.Name,-26} — {item4.Quantity,3} {item4.Unit}");
Console.WriteLine();

Console.WriteLine("— 2. Операції надходження товару на склад (Receive) —");
service.Receive(item1.Id, 15);
var updated1 = service.Find(item1.Id)!;
Console.WriteLine($"  [Прихід]   {updated1.Id} [{updated1.Sku}] {updated1.Name,-26} — новий залишок (+15): {updated1.Quantity} {updated1.Unit}");

service.Receive(item2.Id, 10);
var updated2 = service.Find(item2.Id)!;
Console.WriteLine($"  [Прихід]   {updated2.Id} [{updated2.Sku}] {updated2.Name,-26} — новий залишок (+10): {updated2.Quantity} {updated2.Unit}");

service.Receive(item3.Id, 4);
var updated3 = service.Find(item3.Id)!;
Console.WriteLine($"  [Прихід]   {updated3.Id} [{updated3.Sku}] {updated3.Name,-26} — новий залишок (+4):  {updated3.Quantity} {updated3.Unit}");
Console.WriteLine();

Console.WriteLine("— 3. Операції видачі товару зі складу (Issue) —");
service.Issue(item1.Id, 7);
updated1 = service.Find(item1.Id)!;
Console.WriteLine($"  [Видача]   {updated1.Id} [{updated1.Sku}] {updated1.Name,-26} — залишок після видачі (-7): {updated1.Quantity} {updated1.Unit}");

service.Issue(item2.Id, 3);
updated2 = service.Find(item2.Id)!;
Console.WriteLine($"  [Видача]   {updated2.Id} [{updated2.Sku}] {updated2.Name,-26} — залишок після видачі (-3): {updated2.Quantity} {updated2.Unit}");

service.Issue(item4.Id, 20);
var updated4 = service.Find(item4.Id)!;
Console.WriteLine($"  [Видача]   {updated4.Id} [{updated4.Sku}] {updated3.Name,-26} — залишок після видачі (-20): {updated4.Quantity} {updated4.Unit}");
Console.WriteLine();

Console.WriteLine("— 4. Пошук (Find) та видалення позиції (Delete) —");
var found = service.Find(item3.Id);
Console.WriteLine($"  [Знайдено] {found?.Id} [{found?.Sku}] {found?.Name} (наявність: {found?.Quantity} {found?.Unit})");

bool isDeleted = service.Delete(item4.Id);
Console.WriteLine($"  [Видалено] ID {item4.Id}: результат операції = {isDeleted}");

var checkDeleted = service.Find(item4.Id);
Console.WriteLine($"  [Перевірка видалення] Пошук за ID {item4.Id} повертає null: {checkDeleted is null}");
Console.WriteLine();

// 2. ПЕРЕГЛЯД ПОВНОГО СПИСКУ КАТАЛОГУ (All / List)
Console.WriteLine($"— Список усіх позицій каталогу ({service.All().Count} поз.) —");
Console.WriteLine($"  {"ID",-8} {"SKU",-10} {"Назва",-28} {"К-сть",5} {"Од."}");
Console.WriteLine("  " + new string('-', 58));
foreach (var p in service.All().Take(10))
{
    Console.WriteLine($"  {p.Id,-8} {p.Sku,-10} {p.Name,-28} {p.Quantity,5} {p.Unit}");
}
if (service.All().Count > 10)
{
    Console.WriteLine($"  ... та ще {service.All().Count - 10} позицій");
}
Console.WriteLine();

// 3. СЦЕНАРІЇ ВІДМОВИ, ІНВАРІАНТИ ТА ОБРОБКА ПОМИЛОК
Console.WriteLine("— Перевірка інваріантів та обробки помилок —");

// Помилки сховища (ICatalogStore)
TryDo("Додавання дубліката ID напряму в сховище", () => store.Add(item1));
TryDo("Прихід на неіснуючий ID у сервісі", () => service.Receive("NON-EXISTENT", 10));
TryDo("Списання з неіснуючого ID у сервісі", () => service.Issue("UNKNOWN-ID", 5));
TryDo("Оновлення неіснуючого товару напряму в сховищі", () => store.Update(Product.Create("GHOST", "SKU-GHOST", "Привид", "шт", 1)));

// Інваріанти доменної сутності Product
TryDo("Видача більшої кількості за залишок на складі", () => service.Issue(item1.Id, 1000));
TryDo("Прихід з нульовою кількістю товару", () => service.Receive(item1.Id, 0));
TryDo("Списання з від'ємною кількістю товару", () => service.Issue(item1.Id, -15));
TryDo("Створення товару з порожнім SKU через сервіс", () => service.Add("", "Лопата", "шт", 10));
TryDo("Створення товару з порожньою назвою через сервіс", () => service.Add("SKU-999", "", "шт", 10));
TryDo("Створення товару з від'ємною початковою кількістю", () => service.Add("SKU-999", "Ґрунт", "міш", -20));

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");
    }
}