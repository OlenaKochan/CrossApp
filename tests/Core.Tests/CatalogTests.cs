using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core;
using Core.Domain;
using Core.Reports;
using Core.Services;
using Core.Storage;
using Xunit;
using Core.Events;


namespace Core.Tests
{
    public class CatalogTests
    {
        private static IReadOnlyList<Product> TestItems() => new List<Product>
        {
            Product.Create("1", "SKU-BIG", "Велика труба", "шт", 50),
            Product.Create("2", "SKU-MID", "Середня труба", "шт", 20),
            Product.Create("3", "SKU-LOW", "Мала труба", "шт", 3)
        };

        // 1) Інваріант: відмова і збереження стану
        [Fact]
        public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
        {
            var store = new InMemoryCatalogStore();
            var service = new CatalogService(store);
            var p = service.Add("SKU-1", "Кабель", "м", 10);

            Assert.Throws<ArgumentException>(() => service.Receive(p.Id, -5));
            Assert.Equal(10, store.GetById(p.Id)!.Quantity);
        }

        // 2) Підняття події рівно один раз
        [Fact]
        public void Add_RaisesChangedExactlyOnce()
        {
            var service = new CatalogService(new InMemoryCatalogStore());
            var events = new List<CatalogChangedEventArgs>();
            service.Changed += (s, e) => events.Add(e);

            service.Add("SKU-2", "Гвинт", "шт", 7);

            Assert.Single(events);
            Assert.Equal(CatalogChangeKind.Added, events[0].Kind);
        }

        // 3) Звіт LowStock на кількох наборах ([Theory])
        [Theory]
        [InlineData(10, 1)]
        [InlineData(25, 2)]
        public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
        {
            var count = ReportService.LowStock(TestItems(), threshold).Count;
            Assert.Equal(expected, count);
        }

        // 4) Детермінований TopByQuantity
        [Fact]
        public void TopByQuantity_IsDeterministic()
        {
            var top = ReportService.TopByQuantity(TestItems(), 2).Select(x => x.Sku);
            Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, top);
        }

        // 5) Скасування заздалегідь скасованим токеном
        [Fact]
        public async Task ImportAsync_WhenAlreadyCancelled_Throws()
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var service = new CatalogService(new InMemoryCatalogStore());

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => service.ImportAsync(new JsonCatalogImporter(), "any.json", cts.Token));
        }

        // 6) Зіпсований файл -> DomainException + прибирання у finally
        [Fact]
        public async Task ImportAsync_InvalidData_ThrowsDomainException()
        {
            var tempPath = Path.GetTempFileName();
            try
            {
                await File.WriteAllTextAsync(tempPath, "[{\"id\":\"1\",\"sku\":\"\",\"name\":\"Труба\",\"unit\":\"шт\",\"quantity\":-5}]");
                var service = new CatalogService(new InMemoryCatalogStore());

                await Assert.ThrowsAsync<DomainException>(
                    () => service.ImportAsync(new JsonCatalogImporter(), tempPath));
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }
    }
}