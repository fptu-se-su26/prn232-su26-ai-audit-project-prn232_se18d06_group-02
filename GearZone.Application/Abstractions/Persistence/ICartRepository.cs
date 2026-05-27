using GearZone.Domain.Entities;
using System;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
        Task<Cart?> GetDetailedCartAsync(string userId);
    }
}
