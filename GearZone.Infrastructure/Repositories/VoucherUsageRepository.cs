using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Infrastructure.Repositories
{
    public class VoucherUsageRepository : Repository<VoucherUsage, Guid>, IVoucherUsageRepository
    {
        public VoucherUsageRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
        }

        public async Task<int> GetUsageCountByUserAsync(Guid voucherId, string userId)
        {
            return await Query()
                .CountAsync(vu => vu.VoucherId == voucherId && vu.UserId == userId);
        }
    }
}
