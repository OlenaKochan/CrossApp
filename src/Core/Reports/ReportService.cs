namespace Core.Reports;

using Core.Domain;

public static class ReportService
{
    // 1) Фільтр + сортування зі стабільним другим ключем + проєкція в DTO
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold) =>
        source.Where(p => p.Quantity < threshold)
              .OrderBy(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 2) Топ- позицій за кількістю зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n) =>
        source.OrderByDescending(p => p.Quantity).ThenBy(p => p.Sku, StringComparer.Ordinal)
              .Take(n)
              .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit))
              .ToList();

    // 3) Групування за одиницею виміру з підрахунком кількості та суми
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source) =>
        source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase)
              .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)))
              .OrderByDescending(s => s.TotalQuantity)
              .ToList();

    // 4) Агрегати (кортеж) з безпечною обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {
        var list = source.ToList();
        return list.Count == 0
            ? (0, 0, 0d, 0)
            : (list.Count, 
               list.Sum(p => p.Quantity),
               list.Average(p => p.Quantity), 
               list.Max(p => p.Quantity));
    }
    
    // 5)  Додаткове завдання 1: JOIN з'єднання товарів із переміщеннями (виводить усі факти переміщень)
    public static IReadOnlyList<ProductMovementLine> GetProductMovements(
        IEnumerable<Product> products, 
        IEnumerable<StockMovement> movements) =>
        products.Join(
            movements,
            p => p.Id,
            m => m.ProductId,
            (p, m) => new ProductMovementLine(p.Sku, p.Name, m.Delta, m.OperationType)
        ).ToList();

    // 6)  Додаткове завдання 1: GROUP JOIN з'єднання згрупованих переміщень до товарів (враховує товари навіть без переміщень)
    public static IReadOnlyList<ProductWithMovementsSummary> GetProductsWithGroupedMovements(
        IEnumerable<Product> products, 
        IEnumerable<StockMovement> movements) =>
        products.GroupJoin(
            movements,
            p => p.Id,
            m => m.ProductId,
            (p, mGroup) => new ProductWithMovementsSummary(
                p.Sku, 
                p.Name, 
                mGroup.Count(), 
                mGroup.Sum(m => m.Delta)
            )
        ).ToList();

    // 7) Додаткове завдання 3: Групування через Query Syntax
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {
        var query = from p in source
                    group p by p.Unit into g
                    orderby g.Sum(x => x.Quantity) descending
                    select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Quantity));
        
        return query.ToList();
    }
}

