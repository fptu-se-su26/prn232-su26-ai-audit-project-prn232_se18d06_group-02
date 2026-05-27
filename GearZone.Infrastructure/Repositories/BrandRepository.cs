using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class BrandRepository : Repository<Brand, int>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Brand>> GetAdminBrandsAsync(AdminBrandQueryDto query)
        {
            var dbQuery = _dbSet.Include(b => b.Products).Where(b => !b.IsDeleted).AsQueryable();

            if (query.IsApproved.HasValue)
            {
                dbQuery = dbQuery.Where(b => b.IsApproved == query.IsApproved.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                dbQuery = dbQuery.Where(b => b.Name.ToLower().Contains(search) || b.Slug.ToLower().Contains(search));
            }

            var totalCount = await dbQuery.CountAsync();

            var brands = await dbQuery
                .OrderBy(b => b.Name)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Brand>
            {
                Items = brands,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<List<Brand>> GetAllBrandsListAsync()
        {
            return await _dbSet
                .Where(b => !b.IsDeleted)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<AdminBrandStatsDto> GetBrandStatsAsync()
        {
            var query = _dbSet.Where(b => !b.IsDeleted).AsQueryable();

            return new AdminBrandStatsDto
            {
                TotalBrands = await query.CountAsync(),
                ApprovedBrands = await query.CountAsync(b => b.IsApproved),
                PendingBrands = await query.CountAsync(b => !b.IsApproved)
            };
        }
    }
}
