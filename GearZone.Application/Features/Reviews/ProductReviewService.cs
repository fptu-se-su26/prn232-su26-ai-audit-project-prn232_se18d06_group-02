using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Reviews
{
    public class ProductReviewService : IProductReviewService
    {
        private readonly IProductReviewRepository _productReviewRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductReviewService(
            IProductReviewRepository productReviewRepository,
            IUnitOfWork unitOfWork)
        {
            _productReviewRepository = productReviewRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductReviewSummaryDto> GetProductReviewSummaryAsync(Guid productId)
        {
            return await _productReviewRepository.GetProductReviewSummaryAsync(productId);
        }

        public async Task<PagedResult<ProductReviewListItemDto>> GetProductReviewsAsync(Guid productId, ProductReviewQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 5 : query.PageSize;

            return await _productReviewRepository.GetProductReviewsAsync(productId, query);
        }

        public async Task<EligibleReviewItemDto?> GetEligibleReviewForProductAsync(string userId, Guid productId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _productReviewRepository.GetEligibleReviewForProductAsync(userId, productId, DateTime.UtcNow);
        }

        public async Task<EligibleReviewItemDto?> GetReviewEditorAsync(string userId, Guid orderItemId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _productReviewRepository.GetReviewEditorAsync(userId, orderItemId, DateTime.UtcNow);
        }

        public async Task<PagedResult<MyReviewDto>> GetMyReviewsAsync(string userId, int pageNumber = 1, int pageSize = 10)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            return await _productReviewRepository.GetMyReviewsAsync(userId, pageNumber, pageSize, DateTime.UtcNow);
        }

        public async Task<PagedResult<SellerReviewListItemDto>> GetStoreReviewsAsync(string ownerUserId, SellerReviewQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;

            return await _productReviewRepository.GetStoreReviewsAsync(ownerUserId, query);
        }

        public async Task<ReviewOperationResultDto> CreateOrUpdateReviewAsync(string userId, CreateOrUpdateProductReviewDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return ReviewOperationResultDto.Failure("Please login to review this product.");
            }

            if (dto.Rating < 1 || dto.Rating > 5)
            {
                return ReviewOperationResultDto.Failure("Rating must be between 1 and 5 stars.");
            }

            var reviewEditor = await _productReviewRepository.GetReviewEditorAsync(userId, dto.OrderItemId, DateTime.UtcNow);
            if (reviewEditor == null)
            {
                return ReviewOperationResultDto.Failure("This order item is not eligible for review anymore.");
            }

            var normalizedComment = string.IsNullOrWhiteSpace(dto.Comment)
                ? null
                : dto.Comment.Trim();

            ProductReview? review = await _productReviewRepository.GetByOrderItemIdAsync(dto.OrderItemId);
            if (review == null)
            {
                review = new ProductReview
                {
                    Id = Guid.NewGuid(),
                    OrderItemId = dto.OrderItemId,
                    ProductId = reviewEditor.ProductId,
                    StoreId = reviewEditor.StoreId,
                    BuyerUserId = userId,
                    Rating = dto.Rating,
                    Comment = normalizedComment,
                    CreatedAt = DateTime.UtcNow
                };

                await _productReviewRepository.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                return ReviewOperationResultDto.Success("Your review has been submitted successfully.");
            }

            if (review.BuyerUserId != userId)
            {
                return ReviewOperationResultDto.Failure("You are not allowed to edit this review.");
            }

            review.Rating = dto.Rating;
            review.Comment = normalizedComment;
            review.UpdatedAt = DateTime.UtcNow;

            await _productReviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return ReviewOperationResultDto.Success("Your review has been updated successfully.");
        }

        public async Task<ReviewOperationResultDto> ReplyAsync(string ownerUserId, Guid reviewId, string replyContent)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return ReviewOperationResultDto.Failure("Please login to reply to reviews.");
            }

            var normalizedReply = string.IsNullOrWhiteSpace(replyContent)
                ? null
                : replyContent.Trim();

            if (string.IsNullOrWhiteSpace(normalizedReply))
            {
                return ReviewOperationResultDto.Failure("Reply content cannot be empty.");
            }

            var review = await _productReviewRepository.GetByIdWithStoreAsync(reviewId);
            if (review == null)
            {
                return ReviewOperationResultDto.Failure("Review not found.");
            }

            if (review.Store.OwnerUserId != ownerUserId)
            {
                return ReviewOperationResultDto.Failure("You are not allowed to reply to this review.");
            }

            var utcNow = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(review.SellerReplyContent))
            {
                review.SellerReplyAt = utcNow;
            }
            else
            {
                review.SellerReplyUpdatedAt = utcNow;
            }

            review.SellerReplyContent = normalizedReply;
            review.UpdatedAt = utcNow;

            await _productReviewRepository.UpdateAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return ReviewOperationResultDto.Success("Your reply has been saved successfully.");
        }
    }
}
