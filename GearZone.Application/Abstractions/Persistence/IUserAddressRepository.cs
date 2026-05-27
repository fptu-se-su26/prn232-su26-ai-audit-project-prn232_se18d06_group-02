using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence;

public interface IUserAddressRepository : IRepository<UserAddress, Guid>
{
    Task<IEnumerable<UserAddress>> GetByUserIdAsync(string userId);
    Task<UserAddress?> GetDefaultByUserIdAsync(string userId);
}
