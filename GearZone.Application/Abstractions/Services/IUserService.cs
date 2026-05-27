using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.User.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface IUserService
{
    // Profile
    Task<UserDto?> GetProfileAsync(string userId);
    Task UpdateProfileAsync(string userId, UpdateProfileDto dto);
    Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);

    // Addresses
    Task<IEnumerable<UserAddressDto>> GetUserAddressesAsync(string userId);
    Task<UserAddressDto?> GetAddressByIdAsync(Guid addressId, string userId);
    Task<Guid> AddAddressAsync(string userId, CreateUserAddressDto dto);
    Task UpdateAddressAsync(string userId, UpdateUserAddressDto dto);
    Task DeleteAddressAsync(Guid addressId, string userId);
    Task SetDefaultAddressAsync(Guid addressId, string userId);
}
