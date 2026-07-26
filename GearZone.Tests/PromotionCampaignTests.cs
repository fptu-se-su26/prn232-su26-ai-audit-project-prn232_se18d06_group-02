using System.Linq.Expressions;
using System.Reflection;
using GearZone.Api.Controllers.Seller;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Features.Checkout;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.Orders;
using GearZone.Application.Features.Promotions;
using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using GearZone.Infrastructure;
using GearZone.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Tests;

public sealed class PromotionCampaignTests
{
    private static readonly DateTime Now =
        new(2026, 7, 27, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("paused", PromotionStatus.Paused)]
    [InlineData("upcoming", PromotionStatus.Upcoming)]
    [InlineData("active", PromotionStatus.Active)]
    [InlineData("exhausted", PromotionStatus.Exhausted)]
    [InlineData("expired", PromotionStatus.Expired)]
    public void Status_IsDerivedWithoutPersistedStatus(
        string scenario,
        PromotionStatus expected)
    {
        var campaign = new PromotionCampaign
        {
            IsEnabled = scenario != "paused",
            StartAt = scenario == "upcoming" ? Now.AddMinutes(1) : Now.AddHours(-1),
            EndAt = scenario == "expired" ? Now : Now.AddHours(1),
            TotalQuantityLimit = 10,
            ReservedQuantity = scenario == "exhausted" ? 4 : 0,
            RedeemedQuantity = scenario == "exhausted" ? 6 : 0
        };

        Assert.Equal(expected, campaign.GetStatus(Now));
    }

    [Fact]
    public void RemainingQuantity_NeverBecomesNegative()
    {
        var campaign = new PromotionCampaign
        {
            TotalQuantityLimit = 5,
            ReservedQuantity = 3,
            RedeemedQuantity = 4
        };

        Assert.Equal(0, campaign.RemainingQuantity);
    }

    [Fact]
    public void PercentagePricing_RoundsPerUnitAwayFromZero()
    {
        var service = new PromotionPricingService(
            null!,
            new FixedTimeProvider(Now));
        var result = service.Calculate(
            new ProductVariant { Id = Guid.NewGuid(), Price = 199.99m },
            Campaign(DiscountType.Percent, 12.5m));

        Assert.Equal(199.99m, result.OriginalPrice);
        Assert.Equal(25.00m, result.DiscountPerUnit);
        Assert.Equal(174.99m, result.EffectivePrice);
        Assert.True(result.HasPromotion);
    }

    [Fact]
    public void FixedPricing_IsClampedAtZero()
    {
        var service = new PromotionPricingService(
            null!,
            new FixedTimeProvider(Now));
        var result = service.Calculate(
            new ProductVariant { Id = Guid.NewGuid(), Price = 80m },
            Campaign(DiscountType.FixedAmount, 120m));

        Assert.Equal(80m, result.DiscountPerUnit);
        Assert.Equal(0m, result.EffectivePrice);
    }

    [Fact]
    public void PricingWithoutCampaign_PreservesOriginalPrice()
    {
        var service = new PromotionPricingService(
            null!,
            new FixedTimeProvider(Now));
        var variant = new ProductVariant { Id = Guid.NewGuid(), Price = 250m };

        var result = service.Calculate(variant, null);

        Assert.Equal(250m, result.OriginalPrice);
        Assert.Equal(250m, result.EffectivePrice);
        Assert.Equal(0m, result.DiscountPerUnit);
        Assert.False(result.HasPromotion);
    }

    [Fact]
    public async Task ReservationLifecycle_IsIdempotentAcrossRedeemAndRelease()
    {
        var campaign = Campaign(DiscountType.Percent, 10m);
        campaign.TotalQuantityLimit = 5;
        var campaignRepository = new FakeCampaignRepository(campaign);
        var order = OrderWithPromotedItem(campaign.Id, quantity: 2);
        var reservationRepository = new FakeReservationRepository(
            campaignRepository,
            order.SubOrders.SelectMany(x => x.Items));
        var service = new PromotionLifecycleService(
            campaignRepository,
            reservationRepository,
            new FixedTimeProvider(Now));

        await service.ReserveForOrderAsync(order);
        Assert.Equal(2, campaign.ReservedQuantity);
        Assert.Single(reservationRepository.Items);

        await service.RedeemForOrderAsync(order.Id);
        await service.RedeemForOrderAsync(order.Id);
        Assert.Equal(0, campaign.ReservedQuantity);
        Assert.Equal(2, campaign.RedeemedQuantity);
        Assert.Equal(
            PromotionReservationStatus.Redeemed,
            reservationRepository.Items.Single().Status);

        await service.ReleaseForOrderAsync(order.Id);
        await service.ReleaseForOrderAsync(order.Id);
        Assert.Equal(0, campaign.ReservedQuantity);
        Assert.Equal(0, campaign.RedeemedQuantity);
        Assert.Equal(
            PromotionReservationStatus.Released,
            reservationRepository.Items.Single().Status);
    }

    [Fact]
    public async Task ReservationLifecycle_RejectsOrderExceedingRemainingQuota()
    {
        var campaign = Campaign(DiscountType.Percent, 10m);
        campaign.TotalQuantityLimit = 1;
        var campaignRepository = new FakeCampaignRepository(campaign);
        var order = OrderWithPromotedItem(campaign.Id, quantity: 2);
        var reservationRepository = new FakeReservationRepository(
            campaignRepository,
            order.SubOrders.SelectMany(x => x.Items));
        var service = new PromotionLifecycleService(
            campaignRepository,
            reservationRepository,
            new FixedTimeProvider(Now));

        await Assert.ThrowsAsync<PromotionQuotaExceededException>(
            () => service.ReserveForOrderAsync(order));

        Assert.Equal(0, campaign.ReservedQuantity);
        Assert.Empty(reservationRepository.Items);
    }

    [Fact]
    public async Task RepositoryConditionalReservation_AllowsOnlyLastAvailableUnitOnce()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new SqlitePromotionDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var user = new ApplicationUser
        {
            Id = "promotion-seller",
            UserName = "promotion-seller",
            NormalizedUserName = "PROMOTION-SELLER"
        };
        var store = new Store
        {
            Id = Guid.NewGuid(),
            OwnerUserId = user.Id,
            StoreName = "Promotion test store",
            Slug = $"promotion-test-{Guid.NewGuid():N}",
            Status = StoreStatus.Approved,
            CreatedAt = Now
        };
        var campaign = Campaign(DiscountType.Percent, 10m);
        campaign.StoreId = store.Id;
        campaign.TotalQuantityLimit = 1;
        campaign.RowVersion = new byte[] { 1 };

        db.Users.Add(user);
        db.Stores.Add(store);
        db.PromotionCampaigns.Add(campaign);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var repository = new PromotionCampaignRepository(db);
        var first = await repository.TryReserveQuantityAsync(campaign.Id, 1, Now);
        var second = await repository.TryReserveQuantityAsync(campaign.Id, 1, Now);
        var persisted = await db.PromotionCampaigns
            .AsNoTracking()
            .SingleAsync(x => x.Id == campaign.Id);

        Assert.True(first);
        Assert.False(second);
        Assert.Equal(1, persisted.ReservedQuantity);
    }

    [Fact]
    public async Task VoucherEvaluation_UsesPostPromotionStoreCategoryAndShippingScopes()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new SqlitePromotionDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var parent = new Category
        {
            Id = 90_001,
            Name = "Promotion parent",
            Slug = "promotion-parent",
            IsActive = true
        };
        var child = new Category
        {
            Id = 90_002,
            ParentId = parent.Id,
            Name = "Promotion child",
            Slug = "promotion-child",
            IsActive = true
        };
        var unrelated = new Category
        {
            Id = 90_003,
            Name = "Unrelated",
            Slug = "promotion-unrelated",
            IsActive = true
        };
        var seller = new ApplicationUser
        {
            Id = "voucher-seller",
            UserName = "voucher-seller",
            NormalizedUserName = "VOUCHER-SELLER"
        };
        var store = new Store
        {
            Id = Guid.NewGuid(),
            OwnerUserId = seller.Id,
            StoreName = "Voucher test store",
            Slug = $"voucher-test-{Guid.NewGuid():N}",
            Status = StoreStatus.Approved,
            CreatedAt = Now
        };
        var categoryVoucher = NewVoucher(
            "CATEGORY10",
            VoucherType.OrderDiscount,
            VoucherScope.Platform,
            DiscountType.Percent,
            10m);
        categoryVoucher.CategoryId = parent.Id;
        categoryVoucher.MinOrderAmount = 100m;

        var sellerOrderVoucher = NewVoucher(
            "SELLER50",
            VoucherType.OrderDiscount,
            VoucherScope.Seller,
            DiscountType.FixedAmount,
            50m);
        sellerOrderVoucher.StoreId = store.Id;
        sellerOrderVoucher.MinOrderAmount = 150m;

        var sellerShippingVoucher = NewVoucher(
            "SHIP50",
            VoucherType.ShippingDiscount,
            VoucherScope.Seller,
            DiscountType.Percent,
            50m);
        sellerShippingVoucher.StoreId = store.Id;

        db.Categories.AddRange(parent, child, unrelated);
        db.Users.Add(seller);
        db.Stores.Add(store);
        db.Vouchers.AddRange(
            categoryVoucher,
            sellerOrderVoucher,
            sellerShippingVoucher);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new VoucherService(
            new VoucherRepository(db),
            new VoucherUsageRepository(db),
            new CategoryRepository(db),
            new UnitOfWork(db),
            new FixedTimeProvider(Now));
        var otherStoreId = Guid.NewGuid();
        var context = new VoucherEvaluationContextDto
        {
            Lines =
            {
                new VoucherEvaluationLineDto
                {
                    StoreId = store.Id,
                    CategoryId = child.Id,
                    EffectiveSubtotal = 200m
                },
                new VoucherEvaluationLineDto
                {
                    StoreId = otherStoreId,
                    CategoryId = unrelated.Id,
                    EffectiveSubtotal = 1_000m
                }
            },
            ShippingFees =
            {
                new VoucherShippingFeeDto
                {
                    StoreId = store.Id,
                    ShippingFee = 30m
                },
                new VoucherShippingFeeDto
                {
                    StoreId = otherStoreId,
                    ShippingFee = 100m
                }
            }
        };

        var categoryResult = await service.ValidateVoucherForContextAsync(
            categoryVoucher.Code,
            "buyer",
            context,
            VoucherType.OrderDiscount);
        var sellerOrderResult = await service.ValidateVoucherForContextAsync(
            sellerOrderVoucher.Code,
            "buyer",
            context,
            VoucherType.OrderDiscount);
        var sellerShippingResult = await service.ValidateVoucherForContextAsync(
            sellerShippingVoucher.Code,
            "buyer",
            context,
            VoucherType.ShippingDiscount);

        Assert.True(categoryResult.IsValid);
        Assert.Equal(20m, categoryResult.DiscountAmount);
        Assert.True(sellerOrderResult.IsValid);
        Assert.Equal(50m, sellerOrderResult.DiscountAmount);
        Assert.True(sellerShippingResult.IsValid);
        Assert.Equal(15m, sellerShippingResult.DiscountAmount);

        context.Lines[0].EffectiveSubtotal = 99m;
        var belowMinimum = await service.ValidateVoucherForContextAsync(
            categoryVoucher.Code,
            "buyer",
            context,
            VoucherType.OrderDiscount);
        Assert.False(belowMinimum.IsValid);
    }

    [Fact]
    public void PersistenceModel_ContainsPromotionAndCheckoutUniquenessGuards()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        using var db = new SqlitePromotionDbContext(options);

        AssertUniqueIndex<PromotionReservation>(
            db,
            nameof(PromotionReservation.OrderItemId));
        AssertUniqueIndex<VoucherUsage>(
            db,
            nameof(VoucherUsage.VoucherId),
            nameof(VoucherUsage.OrderId));
        AssertUniqueIndex<Order>(
            db,
            nameof(Order.UserId),
            nameof(Order.CheckoutRequestId));
        AssertUniqueIndex<Voucher>(db, nameof(Voucher.Code));

        var promotionProduct = db.Model.FindEntityType(typeof(PromotionProduct));
        Assert.NotNull(promotionProduct);
        Assert.Equal(
            new[]
            {
                nameof(PromotionProduct.CampaignId),
                nameof(PromotionProduct.ProductId)
            },
            promotionProduct!.FindPrimaryKey()!.Properties.Select(x => x.Name));
    }

    [Fact]
    public async Task CommissionBase_ExcludesSellerVoucherButNotPlatformVoucher()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        await using var db = new SqlitePromotionDbContext(options);
        var service = new OrderService(
            new OrderRepository(db),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!);
        var storeId = Guid.NewGuid();
        var cartItem = CartItemForStore(storeId);
        var request = CheckoutRequest(cartItem.Id);

        var sellerFunded = await service.CreateOrderAsync(
            "buyer",
            request,
            new List<CartItem> { cartItem },
            QuoteFor(
                cartItem,
                VoucherScope.Seller,
                storeId,
                orderDiscount: 30m));
        var platformFunded = await service.CreateOrderAsync(
            "buyer",
            request,
            new List<CartItem> { cartItem },
            QuoteFor(
                cartItem,
                VoucherScope.Platform,
                null,
                orderDiscount: 30m));

        var sellerSubOrder = Assert.Single(sellerFunded.SubOrders);
        Assert.Equal(20m, sellerSubOrder.PromotionDiscountAmount);
        Assert.Equal(30m, sellerSubOrder.SellerVoucherDiscountAmount);
        Assert.Equal(150m, sellerSubOrder.CommissionableAmount);
        Assert.Equal(7.5m, sellerSubOrder.CommissionAmount);
        Assert.Equal(142.5m, sellerSubOrder.NetAmount);

        var platformSubOrder = Assert.Single(platformFunded.SubOrders);
        Assert.Equal(0m, platformSubOrder.SellerVoucherDiscountAmount);
        Assert.Equal(180m, platformSubOrder.CommissionableAmount);
        Assert.Equal(9m, platformSubOrder.CommissionAmount);
        Assert.Equal(171m, platformSubOrder.NetAmount);
    }

    [Fact]
    public async Task SellerCampaignValidation_RejectsWrongStoreOverlapAndResumeConflict()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var db = new SqlitePromotionDbContext(options);
        await db.Database.EnsureCreatedAsync();

        var sellerA = User("campaign-seller-a");
        var sellerB = User("campaign-seller-b");
        var storeA = StoreFor(sellerA, "campaign-store-a");
        var storeB = StoreFor(sellerB, "campaign-store-b");
        var category = new Category
        {
            Id = 91_001,
            Name = "Campaign products",
            Slug = "campaign-products",
            IsActive = true
        };
        var brand = new Brand
        {
            Id = 91_001,
            Name = "Campaign brand",
            Slug = "campaign-brand",
            IsApproved = true,
            CreatedAt = Now
        };
        var productA = ProductFor(storeA.Id, category.Id, brand.Id, "product-a");
        var productB = ProductFor(storeB.Id, category.Id, brand.Id, "product-b");

        db.Users.AddRange(sellerA, sellerB);
        db.Stores.AddRange(storeA, storeB);
        db.Categories.Add(category);
        db.Brands.Add(brand);
        db.Products.AddRange(productA, productB);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        var service = new SellerPromotionService(
            new PromotionCampaignRepository(db),
            new ProductRepository(db),
            new StoreRepository(db),
            new UnitOfWork(db),
            new FixedTimeProvider(Now));

        var first = await service.CreateAsync(
            sellerA.Id,
            CampaignInput("Enabled", productA.Id, true));
        var overlap = await service.CreateAsync(
            sellerA.Id,
            CampaignInput("Overlap", productA.Id, true));
        var wrongStore = await service.CreateAsync(
            sellerA.Id,
            CampaignInput("Wrong store", productB.Id, true));
        var fixedTooHigh = CampaignInput("Too high", productA.Id, false);
        fixedTooHigh.DiscountType = DiscountType.FixedAmount;
        fixedTooHigh.DiscountValue = 101m;
        var invalidFixed = await service.CreateAsync(sellerA.Id, fixedTooHigh);

        var paused = await service.CreateAsync(
            sellerA.Id,
            CampaignInput("Paused", productA.Id, false));
        var pausedId = await db.PromotionCampaigns
            .Where(x => x.Name == "Paused")
            .Select(x => x.Id)
            .SingleAsync();
        var resume = await service.ToggleStatusAsync(sellerA.Id, pausedId);

        Assert.True(first.Success);
        Assert.False(overlap.Success);
        Assert.True(overlap.Conflict);
        Assert.False(wrongStore.Success);
        Assert.False(invalidFixed.Success);
        Assert.True(paused.Success);
        Assert.False(resume.Success);
        Assert.True(resume.Conflict);
    }

    [Fact]
    public void SellerPromotionController_RequiresStoreOwnerRole()
    {
        var authorize = typeof(PromotionsController)
            .GetCustomAttribute<AuthorizeAttribute>();
        var route = typeof(PromotionsController)
            .GetCustomAttributes<RouteAttribute>(inherit: false)
            .Single();

        Assert.NotNull(authorize);
        Assert.Equal("Store Owner", authorize!.Roles);
        Assert.Equal("api/seller/promotions", route!.Template);
    }

    private static PromotionCampaign Campaign(
        DiscountType type,
        decimal value) =>
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Test campaign",
            DiscountType = type,
            DiscountValue = value,
            TotalQuantityLimit = 100,
            StartAt = Now.AddHours(-1),
            EndAt = Now.AddHours(1),
            IsEnabled = true,
            CreatedAt = Now
        };

    private static Voucher NewVoucher(
        string code,
        VoucherType type,
        VoucherScope scope,
        DiscountType discountType,
        decimal discountValue) =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = code,
            Type = type,
            Scope = scope,
            DiscountType = discountType,
            DiscountValue = discountValue,
            UsageLimit = 100,
            MaxUsagePerUser = 1,
            StartAt = Now.AddDays(-1),
            EndAt = Now.AddDays(1),
            IsActive = true,
            Status = VoucherStatus.Active,
            CreatedAt = Now,
            RowVersion = new byte[] { 1 }
        };

    private static CartItem CartItemForStore(Guid storeId)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = "Commission test product"
        };
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Product = product,
            Price = 200m,
            Sku = "COMMISSION-TEST"
        };
        product.Variants.Add(variant);
        return new CartItem
        {
            Id = Guid.NewGuid(),
            VariantId = variant.Id,
            Variant = variant,
            Quantity = 1
        };
    }

    private static CheckoutRequestDto CheckoutRequest(Guid cartItemId) =>
        new()
        {
            RequestId = Guid.NewGuid(),
            CartItemIds = new List<Guid> { cartItemId },
            ShippingInfo = new ShippingInfoDto
            {
                FullName = "Buyer",
                PhoneNumber = "0900000000",
                EmailAddress = "buyer@example.com",
                Address = "Test address"
            },
            PaymentMethod = PaymentMethod.COD
        };

    private static CheckoutQuoteDto QuoteFor(
        CartItem cartItem,
        VoucherScope scope,
        Guid? voucherStoreId,
        decimal orderDiscount) =>
        new()
        {
            Success = true,
            MerchandiseSubtotalBeforePromotion = 200m,
            PromotionDiscountAmount = 20m,
            MerchandiseSubtotal = 180m,
            OrderVoucherDiscountAmount = orderDiscount,
            GrandTotal = 180m - orderDiscount,
            OrderVoucher = new AppliedVoucherDto
            {
                VoucherId = Guid.NewGuid(),
                Code = "ORDER30",
                Name = "Order 30",
                Scope = scope,
                StoreId = voucherStoreId,
                DiscountAmount = orderDiscount
            },
            Lines =
            {
                new CheckoutQuoteLineDto
                {
                    CartItemId = cartItem.Id,
                    VariantId = cartItem.VariantId,
                    ProductId = cartItem.Variant.ProductId,
                    StoreId = cartItem.Variant.Product.StoreId,
                    ProductName = cartItem.Variant.Product.Name,
                    Quantity = cartItem.Quantity,
                    OriginalUnitPrice = 200m,
                    EffectiveUnitPrice = 180m,
                    PromotionDiscountAmount = 20m,
                    PromotionCampaignId = Guid.NewGuid(),
                    PromotionName = "Campaign",
                    LineTotal = 180m
                }
            }
        };

    private static ApplicationUser User(string id) =>
        new()
        {
            Id = id,
            UserName = id,
            NormalizedUserName = id.ToUpperInvariant()
        };

    private static Store StoreFor(ApplicationUser owner, string slug) =>
        new()
        {
            Id = Guid.NewGuid(),
            OwnerUserId = owner.Id,
            StoreName = slug,
            Slug = slug,
            Status = StoreStatus.Approved,
            CreatedAt = Now
        };

    private static Product ProductFor(
        Guid storeId,
        int categoryId,
        int brandId,
        string slug)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            CategoryId = categoryId,
            BrandId = brandId,
            Name = slug,
            Slug = slug,
            BasePrice = 100m,
            Status = ProductStatus.Active,
            CreatedAt = Now
        };
        product.Variants.Add(new ProductVariant
        {
            Id = Guid.NewGuid(),
            ProductId = product.Id,
            Sku = slug.ToUpperInvariant(),
            Price = 100m,
            StockQuantity = 10,
            IsActive = true,
            CreatedAt = Now
        });
        return product;
    }

    private static PromotionCampaignInputDto CampaignInput(
        string name,
        Guid productId,
        bool enabled) =>
        new()
        {
            Name = name,
            DiscountType = DiscountType.Percent,
            DiscountValue = 10m,
            TotalQuantityLimit = 10,
            StartAt = Now.AddMinutes(-10),
            EndAt = Now.AddHours(1),
            IsEnabled = enabled,
            ProductIds = new List<Guid> { productId }
        };

    private static void AssertUniqueIndex<TEntity>(
        ApplicationDbContext db,
        params string[] propertyNames)
    {
        var entityType = db.Model.FindEntityType(typeof(TEntity));
        Assert.NotNull(entityType);
        Assert.Contains(
            entityType!.GetIndexes(),
            index =>
                index.IsUnique &&
                index.Properties.Select(x => x.Name)
                    .SequenceEqual(propertyNames));
    }

    private static Order OrderWithPromotedItem(Guid campaignId, int quantity)
    {
        var order = new Order { Id = Guid.NewGuid() };
        var subOrder = new SubOrder
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            StoreId = Guid.NewGuid(),
            Order = order
        };
        var item = new OrderItem
        {
            Id = Guid.NewGuid(),
            SubOrderId = subOrder.Id,
            SubOrder = subOrder,
            PromotionCampaignId = campaignId,
            Quantity = quantity
        };
        subOrder.Items.Add(item);
        order.SubOrders.Add(subOrder);
        return order;
    }

    private sealed class FixedTimeProvider(DateTime utcNow) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            new(utcNow, TimeSpan.Zero);
    }

    private sealed class SqlitePromotionDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<PromotionCampaign>()
                .Property(x => x.RowVersion)
                .ValueGeneratedNever();
            builder.Entity<Voucher>()
                .Property(x => x.RowVersion)
                .ValueGeneratedNever();
        }
    }

    private sealed class FakeCampaignRepository
        : IPromotionCampaignRepository
    {
        private readonly Dictionary<Guid, PromotionCampaign> _campaigns;

        public FakeCampaignRepository(params PromotionCampaign[] campaigns)
        {
            _campaigns = campaigns.ToDictionary(x => x.Id);
        }

        public Task<PromotionCampaign> AddAsync(
            PromotionCampaign entity,
            CancellationToken ct = default)
        {
            _campaigns[entity.Id] = entity;
            return Task.FromResult(entity);
        }

        public Task AddRangeAsync(
            IEnumerable<PromotionCampaign> items,
            CancellationToken ct = default)
        {
            foreach (var item in items)
            {
                _campaigns[item.Id] = item;
            }

            return Task.CompletedTask;
        }

        public Task<PromotionCampaign> UpdateAsync(PromotionCampaign entity)
        {
            _campaigns[entity.Id] = entity;
            return Task.FromResult(entity);
        }

        public Task DeleteAsync(PromotionCampaign entity)
        {
            _campaigns.Remove(entity.Id);
            return Task.CompletedTask;
        }

        public Task<PromotionCampaign?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default,
            params Expression<Func<PromotionCampaign, object>>[] includes) =>
            Task.FromResult(_campaigns.GetValueOrDefault(id));

        public IQueryable<PromotionCampaign> Query() =>
            _campaigns.Values.AsQueryable();

        public Task<List<PromotionCampaign>> GetActiveForProductsAsync(
            IReadOnlyCollection<Guid> productIds,
            DateTime utcNow,
            CancellationToken ct = default) =>
            Task.FromResult(_campaigns.Values.Where(x =>
                x.GetStatus(utcNow) == PromotionStatus.Active &&
                x.Products.Any(p => productIds.Contains(p.ProductId))).ToList());

        public Task<bool> HasEnabledOverlapAsync(
            Guid storeId,
            IReadOnlyCollection<Guid> productIds,
            DateTime startAt,
            DateTime endAt,
            Guid? excludeCampaignId = null,
            CancellationToken ct = default) =>
            Task.FromResult(_campaigns.Values.Any(x =>
                x.StoreId == storeId &&
                x.IsEnabled &&
                x.Id != excludeCampaignId &&
                x.StartAt < endAt &&
                x.EndAt > startAt &&
                x.Products.Any(p => productIds.Contains(p.ProductId))));

        public Task<bool> TryReserveQuantityAsync(
            Guid campaignId,
            int quantity,
            DateTime utcNow,
            CancellationToken ct = default)
        {
            var campaign = _campaigns.GetValueOrDefault(campaignId);
            if (campaign == null ||
                quantity <= 0 ||
                campaign.GetStatus(utcNow) != PromotionStatus.Active ||
                quantity > campaign.RemainingQuantity)
            {
                return Task.FromResult(false);
            }

            campaign.ReservedQuantity += quantity;
            return Task.FromResult(true);
        }
    }

    private sealed class FakeReservationRepository
        : IPromotionReservationRepository
    {
        private readonly FakeCampaignRepository _campaigns;
        private readonly Dictionary<Guid, OrderItem> _orderItems;

        public FakeReservationRepository(
            FakeCampaignRepository campaigns,
            IEnumerable<OrderItem> orderItems)
        {
            _campaigns = campaigns;
            _orderItems = orderItems.ToDictionary(x => x.Id);
        }

        public List<PromotionReservation> Items { get; } = new();

        public async Task<PromotionReservation> AddAsync(
            PromotionReservation entity,
            CancellationToken ct = default)
        {
            entity.Campaign =
                await _campaigns.GetByIdAsync(entity.CampaignId, ct) ??
                throw new InvalidOperationException();
            entity.OrderItem = _orderItems[entity.OrderItemId];
            Items.Add(entity);
            return entity;
        }

        public async Task AddRangeAsync(
            IEnumerable<PromotionReservation> items,
            CancellationToken ct = default)
        {
            foreach (var item in items)
            {
                await AddAsync(item, ct);
            }
        }

        public Task<PromotionReservation> UpdateAsync(
            PromotionReservation entity) =>
            Task.FromResult(entity);

        public Task DeleteAsync(PromotionReservation entity)
        {
            Items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<PromotionReservation?> GetByIdAsync(
            Guid id,
            CancellationToken ct = default,
            params Expression<Func<PromotionReservation, object>>[] includes) =>
            Task.FromResult(Items.SingleOrDefault(x => x.Id == id));

        public IQueryable<PromotionReservation> Query() =>
            Items.AsQueryable();

        public Task<List<PromotionReservation>> GetByOrderAsync(
            Guid orderId,
            CancellationToken ct = default) =>
            Task.FromResult(Items.Where(x => x.OrderId == orderId).ToList());
    }
}
