using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class StoreFollowRepository : Repository<StoreFollow, Guid>, IStoreFollowRepository
    {
        public StoreFollowRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsAsync(string userId, Guid storeId)
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(f => f.UserId == userId && f.StoreId == storeId);
        }

        public async Task<int> GetFollowerCountAsync(Guid storeId)
        {
            return await _dbSet.AsNoTracking()
                .CountAsync(f => f.StoreId == storeId);
        }

        public async Task<StoreFollow?> GetByUserAndStoreAsync(string userId, Guid storeId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(f => f.UserId == userId && f.StoreId == storeId);
        }
    }
}
