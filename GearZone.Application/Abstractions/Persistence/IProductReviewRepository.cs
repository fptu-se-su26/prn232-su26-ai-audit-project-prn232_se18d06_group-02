using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductReviewRepository : IRepository<ProductReview, Guid>
    {
        Task<ProductReview?> GetByOrderItemIdAsync(Guid orderItemId, CancellationToken ct = default);
        Task<ProductReview?> GetByIdWithStoreAsync(Guid reviewId, CancellationToken ct = default);
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId, CancellationToken ct = default);
        Task<PagedResult<ProductReviewListItemDto>> GetProductReviewsAsync(Guid productId, ProductReviewQueryDto query, CancellationToken ct = default);
        Task<EligibleReviewItemDto?> GetReviewEditorAsync(string userId, Guid orderItemId, DateTime utcNow, CancellationToken ct = default);
        Task<EligibleReviewItemDto?> GetEligibleReviewForProductAsync(string userId, Guid productId, DateTime utcNow, CancellationToken ct = default);
        Task<PagedResult<MyReviewDto>> GetMyReviewsAsync(string userId, int pageNumber, int pageSize, DateTime utcNow, CancellationToken ct = default);
        Task<PagedResult<SellerReviewListItemDto>> GetStoreReviewsAsync(string ownerUserId, SellerReviewQueryDto query, CancellationToken ct = default);
        Task<StoreReviewSnapshotDto> GetStoreReviewSnapshotAsync(Guid storeId, CancellationToken ct = default);
    }
}
