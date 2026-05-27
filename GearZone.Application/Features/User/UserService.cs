using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.User.Dtos;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Application.Features.User;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserAddressRepository _addressRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IUserAddressRepository addressRepository,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _addressRepository = addressRepository;
        _unitOfWork = unitOfWork;
    }

    // Profile Management
    public async Task<UserDto?> GetProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }

    public async Task UpdateProfileAsync(string userId, UpdateProfileDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) throw new InvalidOperationException("User not found.");

        user.FullName = dto.FullName;
        user.PhoneNumber = dto.PhoneNumber;
        if (!string.IsNullOrEmpty(dto.AvatarUrl))
        {
            user.AvatarUrl = dto.AvatarUrl;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return result.Succeeded;
    }

    // Address Management (Ported from UserAddressService)
    public async Task<IEnumerable<UserAddressDto>> GetUserAddressesAsync(string userId)
    {
        var addresses = await _addressRepository.GetByUserIdAsync(userId);
        return addresses.Select(ua => new UserAddressDto
        {
            Id = ua.Id,
            FullName = ua.FullName,
            PhoneNumber = ua.PhoneNumber,
            AddressLine = ua.AddressLine,
            Ward = ua.Ward,
            District = ua.District,
            Province = ua.Province,
            Latitude = ua.Latitude,
            Longitude = ua.Longitude,
            AddressType = ua.AddressType,
            IsDefault = ua.IsDefault
        });
    }

    public async Task<UserAddressDto?> GetAddressByIdAsync(Guid addressId, string userId)
    {
        var ua = await _addressRepository.GetByIdAsync(addressId);
        if (ua == null || ua.UserId != userId) return null;

        return new UserAddressDto
        {
            Id = ua.Id,
            FullName = ua.FullName,
            PhoneNumber = ua.PhoneNumber,
            AddressLine = ua.AddressLine,
            Ward = ua.Ward,
            District = ua.District,
            Province = ua.Province,
            Latitude = ua.Latitude,
            Longitude = ua.Longitude,
            AddressType = ua.AddressType,
            IsDefault = ua.IsDefault
        };
    }

    public async Task<Guid> AddAddressAsync(string userId, CreateUserAddressDto dto)
    {
        if (dto.IsDefault)
        {
            await UnsetDefaultAddressesAsync(userId);
        }
        else
        {
            var existing = await _addressRepository.GetByUserIdAsync(userId);
            if (!existing.Any())
            {
                dto.IsDefault = true;
            }
        }

        var ua = new UserAddress
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            AddressLine = dto.AddressLine,
            Ward = dto.Ward,
            District = dto.District,
            Province = dto.Province,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            AddressType = dto.AddressType,
            IsDefault = dto.IsDefault,
            CreatedAt = DateTime.UtcNow
        };

        await _addressRepository.AddAsync(ua);
        await _unitOfWork.SaveChangesAsync();
        return ua.Id;
    }

    public async Task UpdateAddressAsync(string userId, UpdateUserAddressDto dto)
    {
        var ua = await _addressRepository.GetByIdAsync(dto.Id);
        if (ua == null || ua.UserId != userId) throw new InvalidOperationException("Address not found.");

        if (dto.IsDefault && !ua.IsDefault)
        {
            await UnsetDefaultAddressesAsync(userId);
        }

        ua.FullName = dto.FullName;
        ua.PhoneNumber = dto.PhoneNumber;
        ua.AddressLine = dto.AddressLine;
        ua.Ward = dto.Ward;
        ua.District = dto.District;
        ua.Province = dto.Province;
        ua.Latitude = dto.Latitude;
        ua.Longitude = dto.Longitude;
        ua.AddressType = dto.AddressType;
        ua.IsDefault = dto.IsDefault;
        ua.UpdatedAt = DateTime.UtcNow;

        _addressRepository.UpdateAsync(ua);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAddressAsync(Guid addressId, string userId)
    {
        var ua = await _addressRepository.GetByIdAsync(addressId);
        if (ua == null || ua.UserId != userId) throw new InvalidOperationException("Address not found.");

        await _addressRepository.DeleteAsync(ua);
        await _unitOfWork.SaveChangesAsync();

        if (ua.IsDefault)
        {
            var remaining = await _addressRepository.GetByUserIdAsync(userId);
            var first = remaining.FirstOrDefault();
            if (first != null)
            {
                first.IsDefault = true;
                _addressRepository.UpdateAsync(first);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }

    public async Task SetDefaultAddressAsync(Guid addressId, string userId)
    {
        var ua = await _addressRepository.GetByIdAsync(addressId);
        if (ua == null || ua.UserId != userId) throw new InvalidOperationException("Address not found.");

        if (ua.IsDefault) return;

        await UnsetDefaultAddressesAsync(userId);
        ua.IsDefault = true;
        _addressRepository.UpdateAsync(ua);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task UnsetDefaultAddressesAsync(string userId)
    {
        var currentDefault = await _addressRepository.GetDefaultByUserIdAsync(userId);
        if (currentDefault != null)
        {
            currentDefault.IsDefault = false;
            _addressRepository.UpdateAsync(currentDefault);
        }
    }
}
