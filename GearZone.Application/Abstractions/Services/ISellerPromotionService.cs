using GearZone.Application.Common.Models;
using GearZone.Application.Features.Promotions.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface ISellerPromotionService
    {
        Task<SellerPromotionListDto> GetListAsync(
            string ownerUserId,
            SellerPromotionQueryDto query,
            CancellationToken ct = default);

        Task<PromotionCampaignDto?> GetByIdAsync(
            string ownerUserId,
            Guid campaignId,
            CancellationToken ct = default);

        Task<PagedResult<PromotionProductDto>> GetProductsAsync(
            string ownerUserId,
            string? search,
            int pageNumber,
            int pageSize,
            CancellationToken ct = default);

        Task<(bool Success, string? Error, bool Conflict)> CreateAsync(
            string ownerUserId,
            PromotionCampaignInputDto input,
            CancellationToken ct = default);

        Task<(bool Success, string? Error, bool Conflict)> UpdateAsync(
            string ownerUserId,
            Guid campaignId,
            PromotionCampaignInputDto input,
            CancellationToken ct = default);

        Task<(bool Success, string? Error, bool Conflict)> ToggleStatusAsync(
            string ownerUserId,
            Guid campaignId,
            CancellationToken ct = default);
    }
}
