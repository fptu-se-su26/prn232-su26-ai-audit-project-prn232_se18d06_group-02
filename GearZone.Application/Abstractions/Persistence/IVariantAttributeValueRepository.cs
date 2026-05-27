using GearZone.Domain.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IVariantAttributeValueRepository : IRepository<VariantAttributeValue, Guid>
    {
    }
}
