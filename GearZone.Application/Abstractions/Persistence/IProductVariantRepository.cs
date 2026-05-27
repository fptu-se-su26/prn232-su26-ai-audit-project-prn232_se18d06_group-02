using GearZone.Domain.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductVariantRepository : IRepository<ProductVariant, Guid>
    {
    }
}
