using Core.Dto;

namespace Core.Domain;

public sealed class Product
{
    private int _quantity;

    public string Id { get; }
    public string Sku { get; }
    public string Name { get; }
    public string Unit { get; }
    public string? Note { get; }
    public int Quantity => _quantity;

    // Приватний конструктор
    private Product(string id, string sku, string name, string unit, int quantity, string? note)
    {
        Id = id;
        Sku = sku;
        Name = name;
        Unit = unit;
        _quantity = quantity;
        Note = note;
    }

    // Інваріант 1 та 2: фабричний метод валідує вхідні дані до виклику конструктора
    public static Product Create(string id, string sku, string name, string unit, int quantity, string? note = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Назва товару не може бути порожньою", nameof(name));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("Одиниця виміру обов'язкова", nameof(unit));

        if (quantity < 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Початковий залишок не може бути від'ємним");

        return new Product(
            id.Trim(),
            sku.Trim().ToUpperInvariant(),
            name.Trim(),
            unit.Trim(),
            quantity,
            string.IsNullOrWhiteSpace(note) ? null : note.Trim());
    }

    // Операція приходу: кількість має бути строго додатною
    public void RegisterArrival(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість приходу має бути більшою за нуль");

        _quantity += amount;
    }

    // Інваріант 3: операція видачі (не можна видати більше, ніж є на складі)
    public void Issue(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Кількість видачі має бути більшою за нуль");

        if (amount > _quantity)
            throw new InvalidOperationException($"Не можна видати {amount} {Unit}: поточний залишок для [{Sku}] становить {_quantity} {Unit}");

        _quantity -= amount;
    }

    // Мапінг у DTO минулого тижня та відновлення з нього
    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity, Note);

    public static Product FromDto(ProductDto dto) =>
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity, dto.Note);

    public override string ToString() =>
        $"{Id} [{Sku}] {Name} - {Quantity} {Unit}{(Note is not null ? $" ({Note})" : "")}";
}