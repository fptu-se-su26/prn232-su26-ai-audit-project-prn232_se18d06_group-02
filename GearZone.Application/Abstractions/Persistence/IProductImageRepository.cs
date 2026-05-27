using GearZone.Domain.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductImageRepository : IRepository<ProductImage, Guid>
    {
    }
}
