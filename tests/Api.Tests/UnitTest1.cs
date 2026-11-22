using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Api.Tests;
public class UnitTest1 : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public UnitTest1(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Post_Product_Returns_Created_And_Get_Returns_Ok()
    {
        // Arrange
        var newProduct = new 
        { 
            Sku = "TEST-999", 
            Name = "Тестовий товар", 
            Unit = "шт", 
            Quantity = 15 
        };

        // Act 1 (POST-запит на створення)
        var postResponse = await _client.PostAsJsonAsync("/api/products", newProduct);
        
        // Assert 1 (Перевірка статусу 201 Created та заголовка Location)
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var locationHeader = postResponse.Headers.Location?.ToString();
        Assert.NotNull(locationHeader);

        // Act 2 (GET-запит за отриманою адресою)
        var getResponse = await _client.GetAsync(locationHeader);

        // Assert 2 (Перевірка статусу 200 OK)
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Theory]
    [InlineData("", "Товар без SKU", "шт", 10)]
    [InlineData("SKU-BAD", "", "шт", 5)]
    [InlineData("SKU-BAD-2", "Товар з від'ємною кількістю", "шт", -5)]
    public async Task Post_InvalidProduct_Returns_BadRequest(string sku, string name, string unit, int quantity)
    {
        // Arrange
        var invalidProduct = new { Sku = sku, Name = name, Unit = unit, Quantity = quantity };

        // Act
        var response = await _client.PostAsJsonAsync("/api/products", invalidProduct);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_DuplicateSku_Returns_Conflict()
    {
        // Arrange
        var product = new 
        { 
            Sku = "DUP-001", 
            Name = "Первинний товар", 
            Unit = "кг", 
            Quantity = 10 
        };

        // Act 1 (Створюємо перший раз — успішно)
        var firstResponse = await _client.PostAsJsonAsync("/api/products", product);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act 2 (Створюємо другий раз із тим самим SKU — має бути конфлікт)
        var duplicateResponse = await _client.PostAsJsonAsync("/api/products", product);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);
    }

    [Fact]
    public async Task Get_Products_With_Pagination_And_Search_Returns_Ok()
    {
        // Act
        var response = await _client.GetAsync("/api/products?page=1&pageSize=5&search=Тест");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}