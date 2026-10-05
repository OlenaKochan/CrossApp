namespace Core.Domain;

public static class StockPlacementService
{
    // Інваріант перевіряє стан ДВОХ окремих сутностей одночасно
    public static void PlaceProductToWarehouse(Product product, Warehouse warehouse)
    {
        if (product.Quantity <= 0)
            throw new InvalidOperationException($"Неможливо розмістити товар [{product.Sku}] на складі: залишок товару дорівнює нулю!");

        if (!warehouse.CanAccommodateNewProduct())
            throw new InvalidOperationException($"Склад [{warehouse.Name}] досяг ліміту місткості ({warehouse.MaxCapacity} поз.). Розміщення товару [{product.Sku}] відхилено!");

        warehouse.AddProduct(product.Id);
    }
}