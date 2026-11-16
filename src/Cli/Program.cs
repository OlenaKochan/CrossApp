using System;
using System.Reflection;
using Core.Configuration;
using Core.Domain;
using Core.Storage;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        var envName = Environment.GetEnvironmentVariable("CROSSAPP_ENVIRONMENT") ?? "Development";

        var config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{envName}.json", optional: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .AddEnvironmentVariables(prefix: "CROSSAPP_")
            .AddCommandLine(args)
            .Build();

        var options = config.Get<AppOptions>() ?? new AppOptions();
        
        // Підтримка простої змінної середовища CROSSAPP_DATA_PATH
        var legacy = Environment.GetEnvironmentVariable("CROSSAPP_DATA_PATH");
        var storageOptions = new StorageOptions
        {
            DataPath = string.IsNullOrWhiteSpace(legacy) ? options.Storage.DataPath : legacy,
            FileName = options.Storage.FileName
        };

        var resolvedPath = storageOptions.ResolveFullPath(AppContext.BaseDirectory);

        // Ініціалізуємо сховище за розв'язаним шляхом
        var store = new FileCatalogStore(resolvedPath);

        // Демонстрація: перевірка наявності даних / файлу та додавання тестового запису за потреби
        if (store.List().Count == 0)
        {
            var sampleProduct = Product.Create("a1b2c3d4", "A-100", "Цемент М400", "мішок", 40, "Тестова партія");
            store.Add(sampleProduct);
            Console.WriteLine("Файл сховища не знайдено або він порожній.");
        }

        var apiKey = config["Integration:ApiKey"];

        Console.WriteLine($"Файл даних: {resolvedPath}");
        Console.WriteLine(apiKey is null ? "ApiKey: не заданий" : $"ApiKey: присутній, довжина {apiKey.Length}");
        Console.WriteLine($"Кількість записів у каталозі: {store.List().Count}");
    }
}