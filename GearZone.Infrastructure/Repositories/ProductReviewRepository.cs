using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductReviewRepository : Repository<ProductReview, Guid>, IProductReviewRepository
    {
        public ProductReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ProductReview?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken ct = default)
        {
            return await _dbSet
                .FirstOrDefaultAsync(x => x.OrderItemId == orderItemId && !x.IsDeleted, ct);
        }

        public async Task<ProductReview?> GetByIdWithStoreAsync(Guid reviewId, CancellationToken ct = default)
        {
            return await _dbSet
                .Include(x => x.Store)
                .FirstOrDefaultAsync(x => x.Id == reviewId && !x.IsDeleted, ct);
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.ProductId == productId && !x.IsDeleted);

            var totalReviews = await query.CountAsync(ct);
            var withCommentCount = await query.CountAsync(x => x.Comment != null && x.Comment != string.Empty, ct);
            var averageRating = totalReviews == 0
                ? 0m
                : await query.Select(x => (decimal?)x.Rating).AverageAsync(ct) ?? 0m;

            var grouped = await query
                .GroupBy(x => x.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToListAsync(ct);

            var breakdown = Enumerable.Range(1, 5)
                .OrderByDescending(x => x)
                .Select(rating =>
                {
                    var count = grouped.FirstOrDefault(x => x.Rating == rating)?.Count ?? 0;
                    var percentage = totalReviews == 0
                        ? 0m
                        : Math.Round((decimal)count * 100m / totalReviews, 1);

                    return new ProductReviewBreakdownDto
                    {
                        Rating = rating,
                        Count = count,
                        Percentage = percentage
                    };
                })
                .ToList();

            return new ProductReviewSummaryDto
            {
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews,
                WithCommentCount = withCommentCount,
                Breakdown = breakdown
            };
        }

        public async Task<PagedResult<ProductReviewListItemDto>> GetProductReviewsAsync(Guid productId, ProductReviewQueryDto query, CancellationToken ct = default)
        {
            var reviewQuery = _dbSet
                .AsNoTracking()
                .Where(x => x.ProductId == productId && !x.IsDeleted);

            if (query.Rating.HasValue)
            {
                reviewQuery = reviewQuery.Where(x => x.Rating == query.Rating.Value);
            }

            if (query.WithCommentOnly)
            {
                reviewQuery = reviewQuery.Where(x => x.Comment != null && x.Comment != string.Empty);
            }

            var totalCount = await reviewQuery.CountAsync(ct);

            var items = await reviewQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new ProductReviewListItemDto
                {
                    Id = x.Id,
                    OrderItemId = x.OrderItemId,
                    BuyerDisplayName = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? "User",
                    BuyerAvatarUrl = x.BuyerUser.AvatarUrl,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    VariantName = x.OrderItem.VariantNameSnapshot,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    SellerReplyContent = x.SellerReplyContent,
                    SellerReplyAt = x.SellerReplyAt,
                    SellerReplyUpdatedAt = x.SellerReplyUpdatedAt
                })
                .ToListAsync(ct);

            foreach (var item in items)
            {
                item.BuyerDisplayName = MaskName(item.BuyerDisplayName);
            }

            return new PagedResult<ProductReviewListItemDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<EligibleReviewItemDto?> GetReviewEditorAsync(string userId, Guid orderItemId, DateTime utcNow, CancellationToken ct = default)
        {
            var reviewWindowStart = utcNow.AddDays(-7);

            return await _context.OrderItems
                .AsNoTracking()
                .Where(x =>
                    x.Id == orderItemId &&
                    x.SubOrder.Order.UserId == userId &&
                    x.SubOrder.Status == OrderStatus.Delivered &&
                    (x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt) >= reviewWindowStart)
                .Select(x => new EligibleReviewItemDto
                {
                    OrderItemId = x.Id,
                    ProductId = x.Variant.ProductId,
                    StoreId = x.SubOrder.StoreId,
                    ProductName = x.ProductNameSnapshot,
                    ProductSlug = x.Variant.Product.Slug,
                    ProductImageUrl = x.Variant.Product.Images
                        .Where(img => img.IsPrimary)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                        ?? x.Variant.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                    VariantName = x.VariantNameSnapshot,
                    StoreName = x.SubOrder.Store.StoreName,
                    OrderCode = x.SubOrder.Order.OrderCode,
                    DeliveredAt = x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt,
                    ReviewDeadline = (x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt).AddDays(7),
                    HasExistingReview = x.Review != null && !x.Review.IsDeleted,
                    ReviewId = x.Review != null && !x.Review.IsDeleted ? x.Review.Id : null,
                    ExistingRating = x.Review != null && !x.Review.IsDeleted ? x.Review.Rating : null,
                    ExistingComment = x.Review != null && !x.Review.IsDeleted ? x.Review.Comment : null
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<EligibleReviewItemDto?> GetEligibleReviewForProductAsync(string userId, Guid productId, DateTime utcNow, CancellationToken ct = default)
        {
            var reviewWindowStart = utcNow.AddDays(-7);

            var items = await _context.OrderItems
                .AsNoTracking()
                .Where(x =>
                    x.Variant.ProductId == productId &&
                    x.SubOrder.Order.UserId == userId &&
                    x.SubOrder.Status == OrderStatus.Delivered &&
                    (x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt) >= reviewWindowStart)
                .Select(x => new EligibleReviewItemDto
                {
                    OrderItemId = x.Id,
                    ProductId = x.Variant.ProductId,
                    StoreId = x.SubOrder.StoreId,
                    ProductName = x.ProductNameSnapshot,
                    ProductSlug = x.Variant.Product.Slug,
                    ProductImageUrl = x.Variant.Product.Images
                        .Where(img => img.IsPrimary)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                        ?? x.Variant.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                    VariantName = x.VariantNameSnapshot,
                    StoreName = x.SubOrder.Store.StoreName,
                    OrderCode = x.SubOrder.Order.OrderCode,
                    DeliveredAt = x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt,
                    ReviewDeadline = (x.SubOrder.DeliveredAt ?? x.SubOrder.UpdatedAt ?? x.SubOrder.CreatedAt).AddDays(7),
                    HasExistingReview = x.Review != null && !x.Review.IsDeleted,
                    ReviewId = x.Review != null && !x.Review.IsDeleted ? x.Review.Id : null,
                    ExistingRating = x.Review != null && !x.Review.IsDeleted ? x.Review.Rating : null,
                    ExistingComment = x.Review != null && !x.Review.IsDeleted ? x.Review.Comment : null
                })
                .OrderBy(x => x.HasExistingReview)
                .ThenByDescending(x => x.DeliveredAt)
                .ToListAsync(ct);

            return items.FirstOrDefault();
        }

        public async Task<PagedResult<MyReviewDto>> GetMyReviewsAsync(string userId, int pageNumber, int pageSize, DateTime utcNow, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.BuyerUserId == userId && !x.IsDeleted);

            var totalCount = await query.CountAsync(ct);
            var editableThreshold = utcNow.AddDays(-7);

            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new MyReviewDto
                {
                    Id = x.Id,
                    OrderItemId = x.OrderItemId,
                    ProductId = x.ProductId,
                    ProductName = x.OrderItem.ProductNameSnapshot,
                    ProductSlug = x.Product.Slug,
                    ProductImageUrl = x.Product.Images
                        .Where(img => img.IsPrimary)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                        ?? x.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                    VariantName = x.OrderItem.VariantNameSnapshot,
                    StoreName = x.Store.StoreName,
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt,
                    UpdatedAt = x.UpdatedAt,
                    DeliveredAt = x.OrderItem.SubOrder.DeliveredAt ?? x.OrderItem.SubOrder.UpdatedAt ?? x.OrderItem.SubOrder.CreatedAt,
                    ReviewDeadline = (x.OrderItem.SubOrder.DeliveredAt ?? x.OrderItem.SubOrder.UpdatedAt ?? x.OrderItem.SubOrder.CreatedAt).AddDays(7),
                    CanEdit = (x.OrderItem.SubOrder.DeliveredAt ?? x.OrderItem.SubOrder.UpdatedAt ?? x.OrderItem.SubOrder.CreatedAt) >= editableThreshold,
                    SellerReplyContent = x.SellerReplyContent,
                    SellerReplyAt = x.SellerReplyAt
                })
                .ToListAsync(ct);

            return new PagedResult<MyReviewDto>(items, totalCount, pageNumber, pageSize);
        }

        public async Task<PagedResult<SellerReviewListItemDto>> GetStoreReviewsAsync(string ownerUserId, SellerReviewQueryDto query, CancellationToken ct = default)
        {
            var reviewQuery = _dbSet
                .AsNoTracking()
                .Where(x => x.Store.OwnerUserId == ownerUserId && !x.IsDeleted);

            var filter = (query.Filter ?? "all").Trim().ToLowerInvariant();
            reviewQuery = filter switch
            {
                "unreplied" => reviewQuery.Where(x => x.SellerReplyContent == null || x.SellerReplyContent == string.Empty),
                "replied" => reviewQuery.Where(x => x.SellerReplyContent != null && x.SellerReplyContent != string.Empty),
                _ => reviewQuery
            };

            var totalCount = await reviewQuery.CountAsync(ct);

            var items = await reviewQuery
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(x => new SellerReviewListItemDto
                {
                    Id = x.Id,
                    ProductId = x.ProductId,
                    ProductName = x.OrderItem.ProductNameSnapshot,
                    ProductSlug = x.Product.Slug,
                    ProductImageUrl = x.Product.Images
                        .Where(img => img.IsPrimary)
                        .Select(img => img.ImageUrl)
                        .FirstOrDefault()
                        ?? x.Product.Images.Select(img => img.ImageUrl).FirstOrDefault(),
                    VariantName = x.OrderItem.VariantNameSnapshot,
                    BuyerDisplayName = x.BuyerUser.FullName ?? x.BuyerUser.UserName ?? "User",
                    Rating = x.Rating,
                    Comment = x.Comment,
                    CreatedAt = x.CreatedAt,
                    SellerReplyContent = x.SellerReplyContent,
                    SellerReplyAt = x.SellerReplyAt
                })
                .ToListAsync(ct);

            foreach (var item in items)
            {
                item.BuyerDisplayName = MaskName(item.BuyerDisplayName);
            }

            return new PagedResult<SellerReviewListItemDto>(items, totalCount, query.PageNumber, query.PageSize);
        }

        public async Task<StoreReviewSnapshotDto> GetStoreReviewSnapshotAsync(Guid storeId, CancellationToken ct = default)
        {
            var query = _dbSet
                .AsNoTracking()
                .Where(x => x.StoreId == storeId && !x.IsDeleted);

            var totalReviews = await query.CountAsync(ct);
            var averageRating = totalReviews == 0
                ? 0m
                : await query.Select(x => (decimal?)x.Rating).AverageAsync(ct) ?? 0m;

            return new StoreReviewSnapshotDto
            {
                AverageRating = Math.Round(averageRating, 1),
                TotalReviews = totalReviews
            };
        }

        private static string MaskName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "User";
            }

            var parts = rawName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Trim())
                .Where(part => part.Length > 0)
                .ToList();

            if (parts.Count == 0)
            {
                return "User";
            }

            return string.Join(" ", parts.Select(part => part.Length == 1 ? part : $"{part[0]}***"));
        }
    }
}
