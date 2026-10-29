using System.Collections.Generic;
using System.Linq;
using Core.Domain;
using Core.Reports;
using Xunit;

namespace Core.Tests
{
    public static class TestData
    {
        public static IReadOnlyList<Product> Items() => new List<Product>
        {
            Product.Create("1", "SKU-BIG", "Велика труба", "шт", 50),
            Product.Create("2", "SKU-MID", "Середня труба", "шт", 20),
            Product.Create("3", "SKU-LOW", "Мала труба", "шт", 3)
        };
    }

    public class ReportServiceTests
    {
        [Theory]
        [InlineData(10, 1)]
        [InlineData(25, 2)]
        public void LowStock_CountsOnlyBelowThreshold(int threshold, int expected)
        {
            Assert.Equal(expected, ReportService.LowStock(TestData.Items(), threshold).Count);
        }

        [Fact]
        public void TopByQuantity_IsDeterministic()
        {
            Assert.Equal(new[] { "SKU-BIG", "SKU-MID" }, 
                ReportService.TopByQuantity(TestData.Items(), 2).Select(l => l.Sku));
        }
    }
}