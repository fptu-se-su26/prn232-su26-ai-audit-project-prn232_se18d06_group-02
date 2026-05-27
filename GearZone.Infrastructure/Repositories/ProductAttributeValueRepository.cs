using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductAttributeValueRepository : Repository<ProductAttributeValue, Guid>, IProductAttributeValueRepository
    {
        public ProductAttributeValueRepository(ApplicationDbContext context) : base(context)
        {
        }

        public Task DeleteRangeAsync(IEnumerable<ProductAttributeValue> values)
        {
            _dbSet.RemoveRange(values);
            return Task.CompletedTask;
        }
    }
}

