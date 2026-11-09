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
            // Додаткове завдання: Зв'язані токени скасування (Ctrl+C від користувача + таймаут 10 секунд)
            using var userCts = new CancellationTokenSource();
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(userCts.Token, timeoutCts.Token);

            Console.CancelKeyPress += (s, e) => 
            { 
                e.Cancel = true; 
                userCts.Cancel(); 
            };

            var dataPath = "data/catalog.json";
            var store = new InMemoryCatalogStore();
            var service = new CatalogService(store);

            // Індикатор прогресу через IProgress<int> із відображенням у консолі
            var progress = new Progress<int>(percent =>
            {
                Console.Write($"\rПрогрес імпорту: [{new string('#', percent / 5)}{new string('-', 20 - (percent / 5))}] {percent}%");
            });

            try
            {
                // Додаткові завдання 1 та 3: Потоковий імпорт із підтримкою прогресу та зв'язаного токена
                var importer = new JsonCatalogImporter();
                var added = await service.ImportWithProgressAsync(importer, dataPath, progress, linkedCts.Token);
                
                Console.WriteLine($"\nІмпортовано записів: {added}");
            }
            catch (OperationCanceledException)
            {
                if (timeoutCts.IsCancellationRequested && !userCts.IsCancellationRequested)
                    Console.WriteLine("\nІмпорт скасовано: перевищено ліміт часу (таймаут).");
                else
                    Console.WriteLine("\nІмпорт скасовано користувачем.");
            }
            catch (DomainException ex)
            {
                Console.WriteLine($"\nДані відхилено: {ex.Message}");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"\nФайл не знайдено: {dataPath}");
            }
        }
    }
}