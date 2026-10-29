using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Services;
using Core.Storage;

namespace Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; cts.Cancel(); };

            var dataPath = "data/catalog.json";
            var store = new InMemoryCatalogStore();
            var service = new CatalogService(store);

            try
            {
                var added = await service.ImportAsync(new JsonCatalogImporter(), dataPath, cts.Token);
                Console.WriteLine($"Імпортовано записів: {added}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Імпорт скасовано.");
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"Дані відхилено: {ex.Message}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Файл не знайдено: {dataPath}");
            }
        }
    }
}