using Core.Abstractions;
using Core.Domain;
namespace Core.Storage;

public sealed class CachingCatalogStore(ICatalogStore innerStore) : ICatalogStore
{
    private readonly ICatalogStore _inner = innerStore ?? throw new ArgumentNullException(nameof(innerStore));
    private Dictionary<string, Product>? _cache;

    private Dictionary<string, Product> EnsureCache()
    {
        if (_cache is not null) return _cache;

        _cache = _inner.List().ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        return _cache;
    }

    public IReadOnlyList<Product> List() => EnsureCache().Values.ToList();

    public Product? GetById(string id) => EnsureCache().GetValueOrDefault(id);

    public void Add(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var cache = EnsureCache();

        if (cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує в кеші.");

        _inner.Add(item);
        cache[item.Id] = item;
    }

    public void Update(Product item)
    {
        ArgumentNullException.ThrowIfNull(item);
        var cache = EnsureCache();

        if (!cache.ContainsKey(item.Id))
            throw new InvalidOperationException($"Неможливо оновити: запис з id={item.Id} відсутній у кеші.");

        _inner.Update(item);
        cache[item.Id] = item;
    }

    public bool Remove(string id)
    {
        var cache = EnsureCache();
        bool removed = _inner.Remove(id);
        if (removed)
        {
            cache.Remove(id);
        }
        return removed;
    }

    public IReadOnlyList<Product> Find(Func<Product, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        return EnsureCache().Values.Where(predicate).ToList();
    }
}