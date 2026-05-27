using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductAttributeValueRepository : IRepository<ProductAttributeValue, Guid>
    {
        Task DeleteRangeAsync(IEnumerable<ProductAttributeValue> values);
    }
}

