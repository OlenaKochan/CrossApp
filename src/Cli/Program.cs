using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Events;
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

// 2. Налаштування файлу logs/app.log
var logPath = Path.Combine(AppContext.BaseDirectory, "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

// ПІДПИСНИК 1: іменований локальний метод (вивід у консоль + запис у logs/app.log)
void OnCatalogChanged(object? sender, CatalogChangedEventArgs e)
{
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} залишок {e.QuantityAfter}";
    Console.WriteLine($"  [Log Handler] {line}");
    File.AppendAllText(logPath, line + Environment.NewLine);
}

// ПІДПИСНИК 2: лямбда-вираз із замиканням (closure) над локальним лічильником
int changesCount = 0;
EventHandler<CatalogChangedEventArgs> counterHandler = (sender, e) => changesCount++;

// Підписка двох обробників
service.Changed += OnCatalogChanged;
service.Changed += counterHandler;

// 3. Сценарій: операції з активними підписниками
// Створення об'єктів (Add)
Console.WriteLine("- Створення та реєстрація нових товарів - ");
var item1 = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
Console.WriteLine($"   Створено об'єкт: [ID: {item1.Id}] [{item1.Sku}] \"{item1.Name}\" — {item1.Quantity} {item1.Unit}");

var item2 = service.Add("SKU-102", "Секатор садовий Pro", "шт", 20, "Німеччина");
Console.WriteLine($"   Створено об'єкт: [ID: {item2.Id}] [{item2.Sku}] \"{item2.Name}\" — {item2.Quantity} {item2.Unit}");

var item3 = service.Add("SKU-103", "Шланг для поливу 25м", "бухт", 15);
Console.WriteLine($"   Створено об'єкт: [ID: {item3.Id}] [{item3.Sku}] \"{item3.Name}\" — {item3.Quantity} {item3.Unit}\n");

// Надходження на склад (Receive)
Console.WriteLine("- Реєстрація надходжень на склад - ");
service.Receive(item1.Id, 25);
var updated1 = service.Find(item1.Id)!;
Console.WriteLine($"   Прихід на об'єкт: [{updated1.Sku}] \"{updated1.Name}\" (+25) — новий залишок: {updated1.Quantity} {updated1.Unit}");

service.Receive(item2.Id, 10);
var updated2 = service.Find(item2.Id)!;
Console.WriteLine($"   Прихід на об'єкт: [{updated2.Sku}] \"{updated2.Name}\" (+10) — новий залишок: {updated2.Quantity} {updated2.Unit}\n");


// Видача зі складу (Issue)
Console.WriteLine("- Реєстрація видачі зі складу - ");
service.Issue(item1.Id, 15);
updated1 = service.Find(item1.Id)!;
Console.WriteLine($"   Видача з об'єкта: [{updated1.Sku}] \"{updated1.Name}\" (-15) — залишок: {updated1.Quantity} {updated1.Unit}");

service.Issue(item2.Id, 8);
updated2 = service.Find(item2.Id)!;
Console.WriteLine($"   Видача з об'єкта: [{updated2.Sku}] \"{updated2.Name}\" (-8) — залишок: {updated2.Quantity} {updated2.Unit}\n");

// Пошук (Find) та видалення (Delete)
Console.WriteLine("- Пошук (Find) та видалення товарів - ");
var foundItem = service.Find(item2.Id);
Console.WriteLine($"   Знайдено товар за ID {item2.Id}: [{foundItem?.Sku}] \"{foundItem?.Name}\" ({foundItem?.Quantity} {foundItem?.Unit})");

bool isDeleted = service.Delete(item3.Id);
Console.WriteLine($"   Об'єкт [ID: {item3.Id}] видалено успішно: {isDeleted}");

Console.WriteLine($"Подій отримано лічильником: {changesCount}\n");

// 4. Демонстрація відписки (-=)
Console.WriteLine("— Відписка першого обробника (service.Changed -= OnCatalogChanged) —");
service.Changed -= OnCatalogChanged;

// Тихі операції: події продовжують підніматися, лічильник їх фіксує, але лог мовчить
service.Receive(item1.Id, 5);
updated1 = service.Find(item1.Id)!;
Console.WriteLine($"   Тихий прихід на об'єкт [{updated1.Sku}] \"{updated1.Name}\" (+5) — залишок: {updated1.Quantity} {updated1.Unit}");

service.Issue(item2.Id, 2);
updated2 = service.Find(item2.Id)!;
Console.WriteLine($"   Тиха видача з об'єкта [{updated2.Sku}] \"{updated2.Name}\" (-2) — залишок: {updated2.Quantity} {updated2.Unit}");

Console.WriteLine($"Подій отримано лічильником: {changesCount} (+2 від лямбди)\n");

// 5. Сценарій відмови: операція кидає виняток, подія НЕ викликається
Console.WriteLine("— Сценарій відмови: перевірка, що виняток не породжує подію —");
int countBeforeFailure = changesCount;

TryDo("Спроба видачі кількості, більшої за залишок", () => service.Issue(item1.Id, 998));
TryDo("Спроба приходу з від'ємною кількістю", () => service.Receive(item1.Id, -20));
TryDo("Спроба приходу на неіснуючий ідентифікатор", () => service.Receive("NON-EXISTENT-ID", 10));

Console.WriteLine($"Кількість подій до помилок: {countBeforeFailure}, після: {changesCount}\n");

// 6. Використання делегата Func там, де потрібне значення-результат
Console.WriteLine("— Фільтрація каталогу —");

// Фільтр 1: Товари із залишком менше 20 од.
var lowStock = service.FindAll(p => p.Quantity < 20);
Console.WriteLine($"1. Товари із залишком менше 20 од. (знайдено {lowStock.Count}):");
foreach (var p in lowStock.Take(4))
{
    Console.WriteLine($"   * [{p.Sku}] {p.Name,-25} : {p.Quantity,3} {p.Unit}");
}

// Фільтр 2: Пошук саджанців
var saplings = service.FindAll(p => p.Name.Contains("Саджанець", StringComparison.OrdinalIgnoreCase));
Console.WriteLine($"\n2. Категорія 'Саджанці' (знайдено {saplings.Count}):");
foreach (var p in saplings.Take(4))
{
    Console.WriteLine($"   * [{p.Sku}] {p.Name,-25} : {p.Quantity,3} {p.Unit}");
}

return 0;

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"   {title}: виняток НЕ спрацював!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"   {title}: перехоплено {ex.GetType().Name} — {ex.Message}");
    }
}