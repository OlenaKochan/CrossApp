using Core.Dto;

namespace Core.Domain;

public static class ProductDomainLoader
{
    public static DomainLoadResult<Product> FromImportResult(ImportResult<ProductDto> importResult)
    {
        var entities = new List<Product>();
        var domainErrors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                // Проганяємо через усі інваріанти фабрики Product.Create
                Product product = Product.FromDto(dto);
                entities.Add(product);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                domainErrors.Add($"Товар [{dto.Id}]: порушено доменний інваріант -> {ex.Message}");
            }
        }

        return new DomainLoadResult<Product>(entities, domainErrors);
    }
}