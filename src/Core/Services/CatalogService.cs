using Core.Abstractions;
using Core.Domain;
using Core.Events;
using Core.Dto;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    // Оголошення події згідно з конвенціями .NET
    public event EventHandler<CatalogChangedEventArgs>? Changed;
    // Приватний метод підняття події
    private void OnChanged(Product product, CatalogChangeKind kind, int delta) =>
        Changed?.Invoke(this, new CatalogChangedEventArgs(product.Id, kind, delta, product.Quantity));

    public Product Add(string sku, string name, string unit, int quantity, string? note = null)
    {
        string id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity, note);
        _store.Add(product);

        // Подія піднімається ПІСЛЯ успішного збереження
        OnChanged(product, CatalogChangeKind.Added, quantity);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
        
        // Якщо RegisterArrival кине виняток, рядок нижче не виконається
        OnChanged(product, CatalogChangeKind.Received, quantity);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);

        OnChanged(product, CatalogChangeKind.Issued, -quantity);
    }

    public bool Delete(string id)
    {
        var product = _store.GetById(id);
        if (product is null) return false;

        bool removed = _store.Remove(id);
        if (removed)
        {
            OnChanged(product, CatalogChangeKind.Removed, -product.Quantity);
        }
        return removed;
    }

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);

    // Використання делегата Func там, де потрібне значення (вибірка за критерієм)
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return _store.List().Where(predicate).ToList();
    }

    // Додаткове завдання 2: імпорт із делегатом Action<string> для сповіщення про помилкові рядки
    public int Import(IEnumerable<ProductDto> dtos, Action<string>? onWarning = null)
    {
        ArgumentNullException.ThrowIfNull(dtos);

        int importedCount = 0;
        foreach (var dto in dtos)
        {
            try
            {
                // Спроба відновити сутність і перевірити її інваріанти
                var product = Product.FromDto(dto);
                _store.Add(product);
                OnChanged(product, CatalogChangeKind.Added, product.Quantity);
                importedCount++;
            }
            catch (Exception ex)
            {
                // Замість падіння програми викликаємо Action<string>, якщо підписник його передав
                onWarning?.Invoke($"Пропущено рядок [ID: {dto.Id}, SKU: {dto.Sku}]: {ex.Message}");
            }
        }

        return importedCount;
    }
}