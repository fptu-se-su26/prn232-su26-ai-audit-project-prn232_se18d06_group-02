using System.Text;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Tests;

public sealed class AdminCsvExporterTests
{
    [Fact]
    public void ExportOrders_ProducesExcelCompatibleEscapedUtf8Csv()
    {
        var generatedAt = new DateTime(2026, 7, 26, 4, 5, 6, DateTimeKind.Utc);
        var order = new AdminOrderDto
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            OrderCode = 12345,
            CustomerName = "=HYPERLINK(\"https://example.test\",\"customer\")",
            ReceiverName = "Nguyễn, An",
            ReceiverPhone = "0900000000",
            ShippingAddress = "Line 1\nLine 2",
            GrandTotal = 25000000m,
            PaidAt = null,
            CreatedAt = new DateTime(2026, 7, 25, 10, 30, 0)
        };

        var file = AdminCsvExporter.ExportOrders(new[] { order }, generatedAt);
        var csv = Encoding.UTF8.GetString(file.Content);

        Assert.Equal("text/csv; charset=utf-8", file.ContentType);
        Assert.Equal("admin-orders-20260726-040506.csv", file.FileName);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, file.Content[..3]);
        Assert.Contains("\"Nguyễn, An\"", csv);
        Assert.Contains("\"Line 1\nLine 2\"", csv);
        Assert.Contains("'=HYPERLINK", csv);
        Assert.Contains(",25000000,Unpaid,", csv);
    }

    [Fact]
    public void ExportProducts_WritesExpectedColumnsAndValues()
    {
        var generatedAt = new DateTime(2026, 7, 26, 7, 8, 9, DateTimeKind.Utc);
        var product = new AdminProductDto
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Name = "Wireless \"Pro\" Headphone",
            Sku = "HP-001",
            StoreName = "GearZone Official",
            Category = "Headphones",
            Price = 2100000m,
            Stock = 12,
            Status = "Active",
            CreatedAt = new DateTime(2026, 7, 20, 9, 15, 0),
            ThumbnailUrl = "https://example.test/image.jpg"
        };

        var file = AdminCsvExporter.ExportProducts(new[] { product }, generatedAt);
        var csv = Encoding.UTF8.GetString(file.Content);

        Assert.Equal("admin-products-20260726-070809.csv", file.FileName);
        Assert.Contains("Product ID,Name,SKU,Store,Category,Price (VND),Stock,Status,Created At,Thumbnail URL", csv);
        Assert.Contains("\"Wireless \"\"Pro\"\" Headphone\"", csv);
        Assert.Contains(",HP-001,GearZone Official,Headphones,2100000,12,Active,2026-07-20 09:15:00,", csv);
    }
}
