using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IProductReviewService
    {
        Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId);
        Task<PagedResult<ProductReviewListItemDto>> GetProductReviewsAsync(Guid productId, ProductReviewQueryDto query);
        Task<EligibleReviewItemDto?> GetEligibleReviewForProductAsync(string userId, Guid productId);
        Task<EligibleReviewItemDto?> GetReviewEditorAsync(string userId, Guid orderItemId);
        Task<PagedResult<MyReviewDto>> GetMyReviewsAsync(string userId, int pageNumber = 1, int pageSize = 10);
        Task<PagedResult<SellerReviewListItemDto>> GetStoreReviewsAsync(string ownerUserId, SellerReviewQueryDto query);
        Task<ReviewOperationResultDto> CreateOrUpdateReviewAsync(string userId, CreateOrUpdateProductReviewDto dto);
        Task<ReviewOperationResultDto> ReplyAsync(string ownerUserId, Guid reviewId, string replyContent);
    }
}
