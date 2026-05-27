using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICategoryAttributeRepository : IRepository<CategoryAttribute, int>
    {
        Task<List<CategoryAttribute>> GetByCategoryIdAsync(int categoryId);
        Task DeleteRangeAsync(IEnumerable<CategoryAttribute> attributes);
    }
}

