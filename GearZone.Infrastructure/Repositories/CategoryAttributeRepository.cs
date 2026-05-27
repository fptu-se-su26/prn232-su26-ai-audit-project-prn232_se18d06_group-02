using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class CategoryAttributeRepository : Repository<CategoryAttribute, int>, ICategoryAttributeRepository
    {
        public CategoryAttributeRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<CategoryAttribute>> GetByCategoryIdAsync(int categoryId)
        {
            return await _dbSet
                .Include(a => a.Options.OrderBy(o => o.DisplayOrder))
                .Where(a => a.CategoryId == categoryId)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();
        }

        public async Task DeleteRangeAsync(IEnumerable<CategoryAttribute> attributes)
        {
            _dbSet.RemoveRange(attributes);
        }
    }
}

