using Core.Domain;
namespace Core;
public static class SampleData
{
    public static IReadOnlyList<Product> Products() =>
    [
        Product.Create("P-001", "SKU-001", "Секатор садовий Pro", "шт", 45, "Німеччина"),
        Product.Create("P-002", "SKU-002", "Лопата штикова Fiskars", "шт", 20),
        Product.Create("P-003", "SKU-003", "Граблі віялові", "шт", 15),
        Product.Create("P-004", "SKU-004", "Ґрунт універсальний 20л", "міш", 100),
        Product.Create("P-005", "SKU-005", "Торф кислий 50л", "міш", 60),
        Product.Create("P-006", "SKU-006", "Добриво Осмокот 1кг", "уп", 30),
        Product.Create("P-007", "SKU-007", "Шланг для поливу 25м", "бухт", 25),
        Product.Create("P-008", "SKU-008", "Пістолет-розпилювач", "шт", 35),
        Product.Create("P-009", "SKU-009", "Агроволокно біле 30г/м2", "рул", 40),
        Product.Create("P-010", "SKU-010", "Саджанець Гортензії", "шт", 18),
        Product.Create("P-011", "SKU-011", "Саджанець Лаванди", "шт", 50),
        Product.Create("P-012", "SKU-012", "Саджанець Самшиту", "шт", 30),
        Product.Create("P-013", "SKU-013", "Горщик розсадний 2л", "шт", 200),
        Product.Create("P-014", "SKU-014", "Підпорка для рослин 1.5м", "шт", 80),
        Product.Create("P-015", "SKU-015", "Обприскувач помповий 5л", "шт", 12)
    ];
}