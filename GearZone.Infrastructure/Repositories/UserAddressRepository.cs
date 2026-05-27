using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories;

public class UserAddressRepository : Repository<UserAddress, Guid>, IUserAddressRepository
{
    public UserAddressRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserAddress>> GetByUserIdAsync(string userId)
    {
        return await _context.UserAddresses
            .Where(ua => ua.UserId == userId)
            .OrderByDescending(ua => ua.IsDefault)
            .ThenByDescending(ua => ua.CreatedAt)
            .ToListAsync();
    }

    public async Task<UserAddress?> GetDefaultByUserIdAsync(string userId)
    {
        return await _context.UserAddresses
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.IsDefault);
    }
}
