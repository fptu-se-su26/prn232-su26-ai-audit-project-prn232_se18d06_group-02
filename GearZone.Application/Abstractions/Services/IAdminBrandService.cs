using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminBrandService
    {
        Task<PagedResult<AdminBrandDto>> GetBrandsAsync(AdminBrandQueryDto query);
        Task<AdminBrandDto?> GetBrandByIdAsync(int brandId);
        Task<bool> ApproveBrandAsync(int brandId);
        Task<bool> RejectBrandAsync(int brandId);
        Task<bool> DeleteBrandAsync(int brandId);
        Task<AdminBrandStatsDto> GetBrandStatsAsync();
        Task<bool> CreateBrandAsync(CreateBrandDto dto);
        Task<bool> UpdateBrandAsync(EditBrandDto dto);
        Task<List<AdminBrandDto>> GetAllBrandsListAsync();
    }
}
