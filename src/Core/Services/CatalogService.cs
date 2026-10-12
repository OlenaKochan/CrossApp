using Core.Abstractions;
using Core.Domain;

namespace Core.Services;

public sealed class CatalogService(ICatalogStore store)
{
    private readonly ICatalogStore _store = store ?? throw new ArgumentNullException(nameof(store));

    public Product Add(string sku, string name, string unit, int quantity, string? note = null)
    {
        // Коротка генерація ID на 8 символів без дефісів
        string id = Guid.NewGuid().ToString("N")[..8];
        var product = Product.Create(id, sku, name, unit, quantity, note);
        _store.Add(product);
        return product;
    }

    public void Receive(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.RegisterArrival(quantity);
        _store.Update(product);
    }

    public void Issue(string id, int quantity)
    {
        var product = _store.GetById(id)
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");

        product.Issue(quantity);
        _store.Update(product);
    }

    public bool Delete(string id) => _store.Remove(id);

    public IReadOnlyList<Product> All() => _store.List();

    public Product? Find(string id) => _store.GetById(id);
}