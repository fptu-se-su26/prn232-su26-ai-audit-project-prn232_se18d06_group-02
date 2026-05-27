using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IBrandRepository : IRepository<Brand, int>
    {
        Task<PagedResult<Brand>> GetAdminBrandsAsync(AdminBrandQueryDto query);
        Task<AdminBrandStatsDto> GetBrandStatsAsync();
        Task<List<Brand>> GetAllBrandsListAsync();
    }
}
