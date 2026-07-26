using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GearZone.Tests;

public sealed class AdminReportAggregationTests
{
    [Fact]
    public async Task Reports_UseCorrectRevenueOrderRateAndSellerDenominators()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new ApplicationDbContext(options);
        await db.Database.EnsureCreatedAsync();
        await SeedAsync(db);

        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new AdminReportService(
            new SubOrderRepository(db),
            new OrderItemRepository(db),
            new StoreRepository(db),
            new PaymentRepository(db),
            new ProductReviewRepository(db),
            cache);

        var report = await service.GetOverviewAsync(new AdminReportQueryDto
        {
            Range = "custom",
            From = new DateTime(2026, 7, 1),
            To = new DateTime(2026, 7, 3)
        });

        Assert.Equal(300m, report.PaidGmv.Current);
        Assert.Equal(150m, report.PaidGmv.Previous);
        Assert.Equal(30m, report.PlatformCommission.Current);
        Assert.Equal(270m, report.SellerNetAmount.Current);
        Assert.Equal(1m, report.Orders.Current);
        Assert.Equal(5m, report.UnitsSold.Current);
        Assert.Equal(300m, report.AverageOrderValue.Current);
        Assert.Equal(1m, report.UniqueBuyers.Current);
        Assert.Equal(2m, report.ActiveSellers.Current);
        Assert.Equal(3, report.Trend.Count);
        Assert.Equal(300m, report.RevenueByCategory.Sum(x => x.Revenue));

        var orders = await service.GetOrdersAsync(new AdminReportQueryDto
        {
            Range = "custom", From = new DateTime(2026, 7, 1), To = new DateTime(2026, 7, 3)
        });
        Assert.Equal(2m, orders.Orders.Current);
        Assert.Equal(6m, orders.SubOrders.Current);
        Assert.Equal(2m, orders.PaidSubOrders.Current);
        Assert.Equal(16.67m, orders.CancellationRate);
        Assert.Equal(16.67m, orders.RejectionRate);
        Assert.Equal(16.67m, orders.RefundRate);
        Assert.Equal(10m, orders.AverageFulfillmentHours);
        Assert.Equal(300m, Assert.Single(orders.PaymentMethods).Amount);

        var sellers = await service.GetSellersAsync(new AdminReportQueryDto
        {
            Range = "custom", From = new DateTime(2026, 7, 1), To = new DateTime(2026, 7, 3)
        });
        Assert.Equal(2, sellers.Sellers.TotalCount);
        Assert.Equal("Beta Store", sellers.Sellers.Items[0].StoreName);
        Assert.Null(sellers.Sellers.Items[0].GrowthPct);
        var alpha = sellers.Sellers.Items.Single(x => x.StoreName == "Alpha Store");
        Assert.Equal(-33.33m, alpha.GrowthPct);
        Assert.Equal(20m, alpha.CancellationRate);
        Assert.Equal(20m, alpha.RefundRate);
        Assert.Equal(4m, alpha.AverageRating);

        var searched = await service.GetSellersAsync(new AdminReportQueryDto
        {
            Range = "custom", From = new DateTime(2026, 7, 1), To = new DateTime(2026, 7, 3),
            Search = "alpha", PageSize = 1, SortBy = "rating", SortDirection = "asc"
        });
        Assert.Single(searched.Sellers.Items);
        Assert.Equal("Alpha Store", searched.Sellers.Items[0].StoreName);
    }

    private static async Task SeedAsync(ApplicationDbContext db)
    {
        var owner1 = new ApplicationUser { Id = "owner-1", UserName = "owner1", NormalizedUserName = "OWNER1" };
        var owner2 = new ApplicationUser { Id = "owner-2", UserName = "owner2", NormalizedUserName = "OWNER2" };
        var buyer = new ApplicationUser { Id = "buyer", UserName = "buyer", NormalizedUserName = "BUYER", FullName = "Buyer" };
        var store1 = Store(Guid.NewGuid(), owner1.Id, "Alpha Store", "alpha-store");
        var store2 = Store(Guid.NewGuid(), owner2.Id, "Beta Store", "beta-store");
        var brand = new Brand { Id = 99, Name = "Test Brand", Slug = "test-brand", IsApproved = true };
        var product1 = Product(Guid.NewGuid(), store1.Id, 1, brand.Id, "Keyboard", "test-keyboard");
        var product2 = Product(Guid.NewGuid(), store2.Id, 2, brand.Id, "Mouse", "test-mouse");
        var variant1 = Variant(Guid.NewGuid(), product1.Id, "TEST-KB");
        var variant2 = Variant(Guid.NewGuid(), product2.Id, "TEST-MS");

        var currentOrder = Order(Guid.NewGuid(), 260701001, buyer.Id, new DateTime(2026, 7, 1, 2, 0, 0, DateTimeKind.Utc));
        var paid = SubOrder(Guid.NewGuid(), currentOrder.Id, store1.Id, OrderStatus.Paid, 100m, 10m, 90m, currentOrder.CreatedAt);
        paid.DeliveredAt = currentOrder.CreatedAt.AddHours(10);
        var processing = SubOrder(Guid.NewGuid(), currentOrder.Id, store2.Id, OrderStatus.Processing, 200m, 20m, 180m, currentOrder.CreatedAt);

        var pendingOrder = Order(Guid.NewGuid(), 260701002, buyer.Id, new DateTime(2026, 7, 2, 2, 0, 0, DateTimeKind.Utc));
        var pending = SubOrder(Guid.NewGuid(), pendingOrder.Id, store1.Id, OrderStatus.Pending, 999m, 99m, 900m, pendingOrder.CreatedAt);
        var cancelled = SubOrder(Guid.NewGuid(), pendingOrder.Id, store1.Id, OrderStatus.Cancelled, 50m, 5m, 45m, pendingOrder.CreatedAt);
        var rejected = SubOrder(Guid.NewGuid(), pendingOrder.Id, store1.Id, OrderStatus.Rejected, 60m, 6m, 54m, pendingOrder.CreatedAt);
        var refunded = SubOrder(Guid.NewGuid(), pendingOrder.Id, store1.Id, OrderStatus.Refunded, 70m, 7m, 63m, pendingOrder.CreatedAt);

        var previousOrder = Order(Guid.NewGuid(), 260630001, buyer.Id, new DateTime(2026, 6, 30, 2, 0, 0, DateTimeKind.Utc));
        var previous = SubOrder(Guid.NewGuid(), previousOrder.Id, store1.Id, OrderStatus.Completed, 150m, 15m, 135m, previousOrder.CreatedAt);

        db.AddRange(owner1, owner2, buyer, store1, store2, brand, product1, product2, variant1, variant2,
            currentOrder, paid, processing, pendingOrder, pending, cancelled, rejected, refunded, previousOrder, previous);
        db.Payments.Add(new Payment
        {
            Id = Guid.NewGuid(), OrderId = currentOrder.Id, PaymentCode = "PAY-1", Method = PaymentMethod.PayOS,
            Provider = "PayOS", Amount = 300m, Status = PaymentStatus.Paid,
            CreatedAt = currentOrder.CreatedAt, UpdatedAt = currentOrder.CreatedAt, PaidAt = currentOrder.CreatedAt
        });
        var paidItem = Item(paid.Id, variant1.Id, 2, 100m);
        db.OrderItems.AddRange(
            paidItem,
            Item(processing.Id, variant2.Id, 3, 200m),
            Item(pending.Id, variant1.Id, 20, 999m),
            Item(previous.Id, variant1.Id, 1, 150m));
        db.ProductReviews.Add(new ProductReview
        {
            Id = Guid.NewGuid(), OrderItemId = paidItem.Id, ProductId = product1.Id, StoreId = store1.Id,
            BuyerUserId = buyer.Id, Rating = 4, CreatedAt = currentOrder.CreatedAt
        });
        await db.SaveChangesAsync();
    }

    private static Store Store(Guid id, string ownerId, string name, string slug) => new()
    {
        Id = id, OwnerUserId = ownerId, StoreName = name, Slug = slug, Status = StoreStatus.Approved,
        ApprovedAt = new DateTime(2026, 1, 1), CreatedAt = new DateTime(2025, 1, 1),
        TaxCode = "T", Phone = "0", Email = $"{slug}@test.local", AddressLine = "Test", Province = "HCM",
        BankAccountNumber = "1", BankAccountName = name, BankName = "Test", BankBin = "1"
    };

    private static Product Product(Guid id, Guid storeId, int categoryId, int brandId, string name, string slug) => new()
    {
        Id = id, StoreId = storeId, CategoryId = categoryId, BrandId = brandId, Name = name, Slug = slug,
        Description = "Test", BasePrice = 100m, Status = ProductStatus.Approved, CreatedAt = new DateTime(2025, 1, 1)
    };

    private static ProductVariant Variant(Guid id, Guid productId, string sku) => new()
    {
        Id = id, ProductId = productId, Sku = sku, VariantName = "Default", Price = 100m,
        StockQuantity = 10, IsActive = true, CreatedAt = new DateTime(2025, 1, 1)
    };

    private static Order Order(Guid id, long code, string buyerId, DateTime created) => new()
    {
        Id = id, OrderCode = code, UserId = buyerId, CreatedAt = created, ReceiverName = "Buyer",
        ReceiverPhone = "0", ShippingAddress = "Test"
    };

    private static SubOrder SubOrder(Guid id, Guid orderId, Guid storeId, OrderStatus status,
        decimal subtotal, decimal commission, decimal net, DateTime created) => new()
    {
        Id = id, OrderId = orderId, StoreId = storeId, Status = status, CreatedAt = created,
        Subtotal = subtotal, CommissionAmount = commission, NetAmount = net, CommissionRateSnapshot = 10m
    };

    private static OrderItem Item(Guid subOrderId, Guid variantId, int quantity, decimal total) => new()
    {
        Id = Guid.NewGuid(), SubOrderId = subOrderId, VariantId = variantId, Quantity = quantity,
        LineTotal = total, UnitPriceSnapshot = total / quantity, ProductNameSnapshot = "Product",
        VariantNameSnapshot = "Default", SkuSnapshot = "SKU"
    };
}
