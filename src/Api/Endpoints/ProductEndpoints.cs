using System.Security.Cryptography;
using Microsoft.AspNetCore.Http.HttpResults;
using Api.Contracts;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;

namespace Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/api/products").WithTags("Products");

        // GET: Отримати список усіх товарів
        group.MapGet("/", (ICatalogStore store) =>
            TypedResults.Ok(store.List().Select(p => p.ToDto()).ToList()));

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
        });

        // PUT: Повністю оновити стан товару за ID
        group.MapPut("/{id}", Results<Ok<ProductDto>, NotFound, ValidationProblem> (string id, SaveProductRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors)) 
                return TypedResults.ValidationProblem(errors);

            if (store.GetById(id) is null) 
                return TypedResults.NotFound();

            // Також додаємо 6-й аргумент тут
            var replacement = Product.Create(id, request.Sku, request.Name, request.Unit, request.Quantity, "");
            store.Update(replacement);

            return TypedResults.Ok(replacement.ToDto());
        });

        // DELETE: Видалити товар за ID
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }
}