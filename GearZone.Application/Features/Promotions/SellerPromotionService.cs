using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Promotions.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Promotions
{
    public class SellerPromotionService : ISellerPromotionService
    {
        private static readonly TimeZoneInfo SellerTimeZone =
            ResolveSellerTimeZone();

        private readonly IPromotionCampaignRepository _campaigns;
        private readonly IProductRepository _products;
        private readonly IStoreRepository _stores;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _timeProvider;

        public SellerPromotionService(
            IPromotionCampaignRepository campaigns,
            IProductRepository products,
            IStoreRepository stores,
            IUnitOfWork unitOfWork,
            TimeProvider timeProvider)
        {
            _campaigns = campaigns;
            _products = products;
            _stores = stores;
            _unitOfWork = unitOfWork;
            _timeProvider = timeProvider;
        }

        public async Task<SellerPromotionListDto> GetListAsync(
            string ownerUserId,
            SellerPromotionQueryDto query,
            CancellationToken ct = default)
        {
            query ??= new SellerPromotionQueryDto();
            query.PageNumber = Math.Max(1, query.PageNumber);
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);

            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return new SellerPromotionListDto();
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var source = _campaigns.Query()
                .AsNoTracking()
                .Include(x => x.Products)
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.Variants)
                .Where(x => x.StoreId == store.Id);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim().ToLower();
                source = source.Where(x =>
                    x.Name.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            source = ApplyStatusFilter(source, query.Status, now);
            var total = await source.CountAsync(ct);
            var entities = await source
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(ct);

            var all = await _campaigns.Query()
                .AsNoTracking()
                .Where(x => x.StoreId == store.Id)
                .ToListAsync(ct);

            return new SellerPromotionListDto
            {
                Campaigns = new PagedResult<PromotionCampaignDto>(
                    entities.Select(x => Map(x, now)).ToList(),
                    total,
                    query.PageNumber,
                    query.PageSize),
                Summary = new SellerPromotionSummaryDto
                {
                    TotalCampaigns = all.Count,
                    ActiveCampaigns = all.Count(x => x.GetStatus(now) == PromotionStatus.Active),
                    ReservedUnits = all.Sum(x => x.ReservedQuantity),
                    RedeemedUnits = all.Sum(x => x.RedeemedQuantity)
                }
            };
        }

        public async Task<PromotionCampaignDto?> GetByIdAsync(
            string ownerUserId,
            Guid campaignId,
            CancellationToken ct = default)
        {
            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return null;
            }

            var campaign = await _campaigns.Query()
                .AsNoTracking()
                .Include(x => x.Products)
                    .ThenInclude(x => x.Product)
                        .ThenInclude(x => x.Variants)
                .FirstOrDefaultAsync(x => x.Id == campaignId && x.StoreId == store.Id, ct);

            return campaign == null
                ? null
                : Map(campaign, _timeProvider.GetUtcNow().UtcDateTime);
        }

        public async Task<PagedResult<PromotionProductDto>> GetProductsAsync(
            string ownerUserId,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default)
        {
            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);
            if (store == null)
            {
                return new PagedResult<PromotionProductDto>(new(), 0, pageNumber, pageSize);
            }

            var source = _products.Query()
                .AsNoTracking()
                .Include(x => x.Variants)
                .Where(x =>
                    x.StoreId == store.Id &&
                    !x.IsDeleted &&
                    (x.Status == ProductStatus.Active || x.Status == ProductStatus.Approved));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var normalized = search.Trim().ToLower();
                source = source.Where(x => x.Name.ToLower().Contains(normalized));
            }

            var total = await source.CountAsync(ct);
            var items = await source.OrderBy(x => x.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PromotionProductDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    BasePrice = x.BasePrice,
                    MinimumVariantPrice = x.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .Select(v => (decimal?)v.Price)
                        .Min() ?? x.BasePrice
                })
                .ToListAsync(ct);

            return new PagedResult<PromotionProductDto>(items, total, pageNumber, pageSize);
        }

        public async Task<(bool Success, string? Error, bool Conflict)> CreateAsync(
            string ownerUserId,
            PromotionCampaignInputDto input,
            CancellationToken ct = default)
        {
            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.", false);
            }

            var validation = await ValidateAsync(store.Id, input, null, ct);
            if (validation != null)
            {
                return validation.Value;
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var campaign = new PromotionCampaign
            {
                Id = Guid.NewGuid(),
                StoreId = store.Id,
                Name = input.Name.Trim(),
                Description = Clean(input.Description),
                DiscountType = input.DiscountType,
                DiscountValue = input.DiscountValue,
                TotalQuantityLimit = input.TotalQuantityLimit,
                StartAt = NormalizeUtc(input.StartAt),
                EndAt = NormalizeUtc(input.EndAt),
                IsEnabled = input.IsEnabled,
                CreatedAt = now,
                Products = input.ProductIds.Distinct().Select(productId => new PromotionProduct
                {
                    ProductId = productId
                }).ToList()
            };

            await _campaigns.AddAsync(campaign, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            return (true, null, false);
        }

        public async Task<(bool Success, string? Error, bool Conflict)> UpdateAsync(
            string ownerUserId,
            Guid campaignId,
            PromotionCampaignInputDto input,
            CancellationToken ct = default)
        {
            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.", false);
            }

            var campaign = await _campaigns.Query()
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == campaignId && x.StoreId == store.Id, ct);
            if (campaign == null)
            {
                return (false, "Promotion campaign not found.", false);
            }

            var validation = await ValidateAsync(store.Id, input, campaignId, ct);
            if (validation != null)
            {
                return validation.Value;
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var canFullyEdit = now < campaign.StartAt &&
                               campaign.ReservedQuantity == 0 &&
                               campaign.RedeemedQuantity == 0;

            if (!canFullyEdit)
            {
                var sameProducts = campaign.Products.Select(x => x.ProductId).Order()
                    .SequenceEqual(input.ProductIds.Distinct().Order());
                if (!sameProducts ||
                    campaign.DiscountType != input.DiscountType ||
                    campaign.DiscountValue != input.DiscountValue ||
                    NormalizeUtc(input.StartAt) != campaign.StartAt)
                {
                    return (false,
                        "Products, discount and start time cannot be changed after a campaign has started.",
                        true);
                }

                if (NormalizeUtc(input.EndAt) < campaign.EndAt)
                {
                    return (false, "An active campaign can only have its end time extended.", true);
                }

                if (input.TotalQuantityLimit < campaign.TotalQuantityLimit)
                {
                    return (false, "An active campaign quantity limit can only be increased.", true);
                }
            }

            var allocated = campaign.ReservedQuantity + campaign.RedeemedQuantity;
            if (input.TotalQuantityLimit < allocated)
            {
                return (false, "Quantity limit cannot be lower than allocated quantity.", true);
            }

            campaign.Name = input.Name.Trim();
            campaign.Description = Clean(input.Description);
            campaign.EndAt = NormalizeUtc(input.EndAt);
            campaign.TotalQuantityLimit = input.TotalQuantityLimit;
            campaign.IsEnabled = input.IsEnabled;
            campaign.UpdatedAt = now;

            if (canFullyEdit)
            {
                campaign.DiscountType = input.DiscountType;
                campaign.DiscountValue = input.DiscountValue;
                campaign.StartAt = NormalizeUtc(input.StartAt);
                campaign.Products.Clear();
                foreach (var productId in input.ProductIds.Distinct())
                {
                    campaign.Products.Add(new PromotionProduct
                    {
                        CampaignId = campaign.Id,
                        ProductId = productId
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            return (true, null, false);
        }

        public async Task<(bool Success, string? Error, bool Conflict)> ToggleStatusAsync(
            string ownerUserId,
            Guid campaignId,
            CancellationToken ct = default)
        {
            var store = await _stores.GetStoreByOwnerIdAsync(ownerUserId);
            if (store == null)
            {
                return (false, "Store not found.", false);
            }

            var campaign = await _campaigns.Query()
                .Include(x => x.Products)
                .FirstOrDefaultAsync(x => x.Id == campaignId && x.StoreId == store.Id, ct);
            if (campaign == null)
            {
                return (false, "Promotion campaign not found.", false);
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            if (!campaign.IsEnabled)
            {
                if (campaign.EndAt <= now)
                {
                    return (false, "An expired campaign cannot be resumed.", true);
                }

                var productIds = campaign.Products.Select(x => x.ProductId).ToArray();
                if (await _campaigns.HasEnabledOverlapAsync(
                        store.Id,
                        productIds,
                        campaign.StartAt,
                        campaign.EndAt,
                        campaign.Id,
                        ct))
                {
                    return (false, "Another enabled campaign overlaps these products and dates.", true);
                }
            }

            campaign.IsEnabled = !campaign.IsEnabled;
            campaign.UpdatedAt = now;
            await _unitOfWork.SaveChangesAsync(ct);
            return (true, null, false);
        }

        private async Task<(bool Success, string? Error, bool Conflict)?> ValidateAsync(
            Guid storeId,
            PromotionCampaignInputDto input,
            Guid? excludeId,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(input.Name))
            {
                return (false, "Campaign name is required.", false);
            }

            var start = NormalizeUtc(input.StartAt);
            var end = NormalizeUtc(input.EndAt);
            if (end <= start)
            {
                return (false, "End time must be after start time.", false);
            }

            if (input.TotalQuantityLimit < 1 || input.DiscountValue <= 0)
            {
                return (false, "Discount and quantity limit must be greater than zero.", false);
            }

            if (input.DiscountType == DiscountType.Percent && input.DiscountValue > 100)
            {
                return (false, "Percentage discount cannot exceed 100%.", false);
            }

            var productIds = input.ProductIds.Distinct().ToArray();
            if (productIds.Length == 0)
            {
                return (false, "Select at least one product.", false);
            }

            var products = await _products.Query()
                .AsNoTracking()
                .Include(x => x.Variants)
                .Where(x => productIds.Contains(x.Id))
                .ToListAsync(ct);

            if (products.Count != productIds.Length ||
                products.Any(x =>
                    x.StoreId != storeId ||
                    x.IsDeleted ||
                    (x.Status != ProductStatus.Active &&
                     x.Status != ProductStatus.Approved) ||
                    !x.Variants.Any(v => v.IsActive && !v.IsDeleted)))
            {
                return (false, "One or more products do not belong to this store.", false);
            }

            if (input.DiscountType == DiscountType.FixedAmount)
            {
                var minimumPrice = products
                    .SelectMany(x => x.Variants.Where(v => v.IsActive && !v.IsDeleted))
                    .Select(x => (decimal?)x.Price)
                    .Min();
                if (minimumPrice.HasValue && input.DiscountValue > minimumPrice.Value)
                {
                    return (false, "Fixed discount cannot exceed the lowest active variant price.", false);
                }
            }

            if (input.IsEnabled && await _campaigns.HasEnabledOverlapAsync(
                    storeId, productIds, start, end, excludeId, ct))
            {
                return (false, "Another enabled campaign overlaps these products and dates.", true);
            }

            return null;
        }

        private static IQueryable<PromotionCampaign> ApplyStatusFilter(
            IQueryable<PromotionCampaign> query,
            PromotionStatus? status,
            DateTime now)
        {
            return status switch
            {
                PromotionStatus.Paused => query.Where(x => !x.IsEnabled && x.EndAt > now),
                PromotionStatus.Upcoming => query.Where(x => x.IsEnabled && x.StartAt > now),
                PromotionStatus.Active => query.Where(x =>
                    x.IsEnabled && x.StartAt <= now && x.EndAt > now &&
                    x.ReservedQuantity + x.RedeemedQuantity < x.TotalQuantityLimit),
                PromotionStatus.Exhausted => query.Where(x =>
                    x.IsEnabled && x.StartAt <= now && x.EndAt > now &&
                    x.ReservedQuantity + x.RedeemedQuantity >= x.TotalQuantityLimit),
                PromotionStatus.Expired => query.Where(x => x.EndAt <= now),
                _ => query
            };
        }

        private static PromotionCampaignDto Map(PromotionCampaign campaign, DateTime now) => new()
        {
            Id = campaign.Id,
            Name = campaign.Name,
            Description = campaign.Description,
            DiscountType = campaign.DiscountType,
            DiscountValue = campaign.DiscountValue,
            TotalQuantityLimit = campaign.TotalQuantityLimit,
            ReservedQuantity = campaign.ReservedQuantity,
            RedeemedQuantity = campaign.RedeemedQuantity,
            RemainingQuantity = campaign.RemainingQuantity,
            StartAt = campaign.StartAt,
            EndAt = campaign.EndAt,
            IsEnabled = campaign.IsEnabled,
            Status = campaign.GetStatus(now),
            CreatedAt = campaign.CreatedAt,
            UpdatedAt = campaign.UpdatedAt,
            Products = campaign.Products.Select(x => new PromotionProductDto
            {
                Id = x.ProductId,
                Name = x.Product.Name,
                BasePrice = x.Product.BasePrice,
                MinimumVariantPrice = x.Product.Variants
                    .Where(v => v.IsActive && !v.IsDeleted)
                    .Select(v => (decimal?)v.Price)
                    .Min() ?? x.Product.BasePrice
            }).OrderBy(x => x.Name).ToList()
        };

        private static DateTime NormalizeUtc(DateTime value) =>
            value.Kind == DateTimeKind.Unspecified
                ? TimeZoneInfo.ConvertTimeToUtc(value, SellerTimeZone)
                : value.ToUniversalTime();

        private static TimeZoneInfo ResolveSellerTimeZone()
        {
            foreach (var id in new[] { "Asia/Ho_Chi_Minh", "SE Asia Standard Time" })
            {
                try
                {
                    return TimeZoneInfo.FindSystemTimeZoneById(id);
                }
                catch (TimeZoneNotFoundException)
                {
                }
                catch (InvalidTimeZoneException)
                {
                }
            }

            return TimeZoneInfo.CreateCustomTimeZone(
                "Vietnam",
                TimeSpan.FromHours(7),
                "Vietnam",
                "Vietnam");
        }

        private static string? Clean(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
