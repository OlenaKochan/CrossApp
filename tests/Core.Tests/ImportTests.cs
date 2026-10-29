using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests
{
    public class ImportTests
    {
        [Fact]
        public async Task ImportAsync_WhenAlreadyCancelled_Throws()
        {
            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();
            var service = new CatalogService(new InMemoryCatalogStore());
            var tempPath = Path.GetTempFileName();
            try
            {
                await Assert.ThrowsAnyAsync<OperationCanceledException>(
                    () => service.ImportAsync(new JsonCatalogImporter(), tempPath, cts.Token));
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

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