namespace Core.Domain;

public sealed class Warehouse
{
    private readonly List<string> _storedProductIds = [];

    public string Id { get; }
    public string Name { get; }
    public int MaxCapacity { get; }
    public IReadOnlyList<string> StoredProductIds => _storedProductIds.AsReadOnly();

    private Warehouse(string id, string name, int maxCapacity)
    {
        Id = id;
        Name = name;
        MaxCapacity = maxCapacity;
    }

    public static Warehouse Create(string id, string name, int maxCapacity)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Id складу обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва складу обов'язкова", nameof(name));
        if (maxCapacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxCapacity), "Місткість складу має бути більшою за нуль");

        return new Warehouse(id.Trim(), name.Trim(), maxCapacity);
    }

    public bool CanAccommodateNewProduct() => _storedProductIds.Count < MaxCapacity;

    public void AddProduct(string productId)
    {
        if (!CanAccommodateNewProduct())
            throw new InvalidOperationException($"Склад {Name} заповнений (ліміт: {MaxCapacity} позицій)!");

        _storedProductIds.Add(productId);
    }
}