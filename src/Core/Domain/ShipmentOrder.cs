namespace Core.Domain;

public sealed class ShipmentOrder
{
    public string Id { get; }
    public ShipmentStatus Status { get; private set; }
    public DateTime CreatedAt { get; }

    private ShipmentOrder(string id, DateTime createdAt)
    {
        Id = id;
        Status = ShipmentStatus.Draft;
        CreatedAt = createdAt;
    }

    public static ShipmentOrder Create(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Номер накладної обов'язковий", nameof(id));

        return new ShipmentOrder(id.Trim(), DateTime.UtcNow);
    }

    // Перевірка допустимості переходу через switch expression
    public void ChangeStatus(ShipmentStatus newStatus)
    {
        bool isTransitionAllowed = (Status, newStatus) switch
        {
            // З чернетки можна підтвердити або скасувати
            (ShipmentStatus.Draft, ShipmentStatus.Confirmed) => true,
            (ShipmentStatus.Draft, ShipmentStatus.Cancelled) => true,

            // Підтверджену накладну можна тільки скасувати
            (ShipmentStatus.Confirmed, ShipmentStatus.Cancelled) => true,

            // Перехід у той самий статус не має сенсу
            var (current, target) when current == target => false,

            // Зі скасованого стану повернення немає, або будь-які інші невалідні переходи
            _ => false
        };

        if (!isTransitionAllowed)
        {
            throw new InvalidOperationException(
                $"Неприпустимий перехід накладної {Id}: неможливо змінити стан з '{Status}' на '{newStatus}'!");
        }

        Status = newStatus;
    }

    public override string ToString() => $"Накладна {Id} | Стан: {Status} (створено: {CreatedAt:yyyy-MM-dd HH:mm})";
}