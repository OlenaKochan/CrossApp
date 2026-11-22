using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Api.Filters;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // Додаткове 1 : GET Отримати список товарів з пагінацією та пошуком
        group.MapGet("/", (int? page, int? pageSize, string? search, ICatalogStore store) =>
        {
            int p = page ?? 1;
            int size = pageSize ?? 10;
            var allItems = store.List();
            // Фільтрація за пошуком (якщо задано)
            if (!string.IsNullOrWhiteSpace(search))
            {
                allItems = allItems.Where(x => 
                    x.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    x.Sku.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }
            var total = allItems.Count;
            var items = allItems
                .Skip((p - 1) * size)
                .Take(size)
                .Select(p => p.ToDto())
                .ToList();
            return TypedResults.Ok(new { items, total });
        });

        // GET: Отримати товар за ID
        group.MapGet("/{id}", Results<Ok<ProductDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(found.ToDto())
                : TypedResults.NotFound());

        // POST: Створити новий товар
        group.MapPost("/", Results<Created<ProductDto>, ValidationProblem> (SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors)) 
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var created = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity, "");
            store.Add(created);

            return TypedResults.Created($"/api/products/{created.Id}", created.ToDto());
        })   //Додаткове 2 Підключаємо фільтр FluentValidation
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // PUT: Повністю оновити стан товару за ID
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (store.GetById(id) is null) 
                return TypedResults.NotFound();

            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity, "");
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToDto());
        })  //Додаткове 2 Підключаємо фільтр FluentValidation
        .AddEndpointFilter<ValidationFilter<SaveProductRequest>>();

        // DELETE: Видалити товар за ID
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}