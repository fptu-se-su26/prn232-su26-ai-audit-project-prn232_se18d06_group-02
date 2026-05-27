using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminVoucherService
    {
        Task<bool> CreateVoucherAsync(CreateVoucherDto dto);
        Task<PagedResult<AdminVoucherDto>> GetPaginatedVouchersAsync(AdminVoucherQueryDto query);
        Task<AdminVoucherSummaryDto> GetVoucherSummaryAsync();
        Task<AdminVoucherDto?> GetVoucherByIdAsync(Guid id);
        Task<bool> UpdateVoucherAsync(Guid id, UpdateVoucherDto dto);
        Task<bool> ToggleVoucherStatusAsync(Guid id);
    }
}
