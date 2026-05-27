using GearZone.Application.Common.Models;
using GearZone.Application.Features.Seller.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface ISellerVoucherService
    {
        Task<PagedResult<SellerVoucherDto>> GetPaginatedVouchersAsync(string ownerUserId, SellerVoucherQueryDto query);
        Task<SellerVoucherSummaryDto> GetVoucherSummaryAsync(string ownerUserId);
        Task<SellerVoucherDto?> GetVoucherByIdAsync(string ownerUserId, Guid voucherId);
        Task<(bool Success, string? Error)> CreateVoucherAsync(string ownerUserId, SellerCreateVoucherDto dto);
        Task<(bool Success, string? Error)> UpdateVoucherAsync(string ownerUserId, Guid voucherId, SellerUpdateVoucherDto dto);
        Task<(bool Success, string? Error)> ToggleVoucherStatusAsync(string ownerUserId, Guid voucherId);
    }
}
