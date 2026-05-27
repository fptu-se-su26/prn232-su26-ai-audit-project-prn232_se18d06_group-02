using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;

namespace GearZone.Infrastructure.Repositories
{
    public class VariantAttributeValueRepository : Repository<VariantAttributeValue, Guid>, IVariantAttributeValueRepository
    {
        public VariantAttributeValueRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
