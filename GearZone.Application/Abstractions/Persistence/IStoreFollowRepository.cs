using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IStoreFollowRepository : IRepository<StoreFollow, Guid>
    {
        Task<bool> ExistsAsync(string userId, Guid storeId);
        Task<int> GetFollowerCountAsync(Guid storeId);
        Task<StoreFollow?> GetByUserAndStoreAsync(string userId, Guid storeId);
    }
}
