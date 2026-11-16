using Scalar.AspNetCore;
using Api.Endpoints;
using Core.Abstractions;
using Core.Storage;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICatalogStore, InMemoryCatalogStore>();

// Налаштування для виведення кирилиці у JSON 
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
});

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Реєструємо кодування для Windows-1251
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// middleware: розпізнає джерело та конвертує кодування за потреби
app.Use(async (context, next) =>
{
    if (context.Request.ContentType != null && 
        context.Request.ContentType.Contains("application/json") &&
        (HttpMethods.IsPost(context.Request.Method) || HttpMethods.IsPut(context.Request.Method)))
    {
        using var ms = new MemoryStream();
        await context.Request.Body.CopyToAsync(ms);
        var rawBytes = ms.ToArray();

        if (rawBytes.Length > 0)
        {
            // Перевіряємо, чи є запит валідним UTF-8
            if (!IsValidUtf8(rawBytes))
            {
                // Якщо це Git Bash / Windows-1251, конвертуємо у UTF-8
                var bodyString = Encoding.GetEncoding(1251).GetString(rawBytes);
                var fixedBytes = Encoding.UTF8.GetBytes(bodyString);
                context.Request.Body = new MemoryStream(fixedBytes);
                context.Request.ContentLength = fixedBytes.Length;
            }
            else
            {
                // Якщо вже UTF-8 (Scalar), просто повертаємо потік назад без змін
                context.Request.Body = new MemoryStream(rawBytes);
                context.Request.ContentLength = rawBytes.Length;
            }
        }
    }
    await next();
});

// Допоміжна функція для перевірки валідності UTF-8
bool IsValidUtf8(byte[] bytes)
{
    try {
        var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true); utf8.GetString(bytes); return true;
    }
    catch { return false; }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapProducts();

app.Run();