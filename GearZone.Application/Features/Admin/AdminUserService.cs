using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GearZone.Application.Abstractions;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Abstractions.Services;

namespace GearZone.Application.Features.Admin
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public AdminUserService(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<PagedResult<UserDto>> GetPaginatedUsersAsync(UserQueryDto query)
        {
            return await _userRepository.GetUsersAsync(query);
        }

        public async Task<List<string>> GetAllRolesAsync()
        {
            return await Task.FromResult(_roleManager.Roles.Select(r => r.Name!).ToList());
        }

        public async Task<IdentityResult> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
            {
                return IdentityResult.Failed(new IdentityError { Description = $"Email {dto.Email} is already taken." });
            }

            var user = _mapper.Map<ApplicationUser>(dto);

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (result.Succeeded && !string.IsNullOrWhiteSpace(dto.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
                return roleResult;
            }

            return result;
        }

        public async Task<UserDto?> GetUserByIdAsync(string userId)
        {
            return await _userRepository.GetUserByIdAsync(userId);
        }

        public async Task<IdentityResult> UpdateUserAsync(EditUserDto dto)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;
            user.IsActive = dto.IsActive;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return result;

            var currentRoles = await _userManager.GetRolesAsync(user);
            if (!currentRoles.Any(r => string.Equals(r, dto.Role, StringComparison.OrdinalIgnoreCase)))
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded) return removeResult;

                var addResult = await _userManager.AddToRoleAsync(user, dto.Role);
                if (!addResult.Succeeded) return addResult;
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SoftDeleteUserAsync(string userId, string deletedByUserId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.IsDeleted = true;
            user.DeletedAt = System.DateTime.UtcNow;
            user.DeletedBy = deletedByUserId;
            user.IsActive = false;

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> RestoreUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            user.DeletedBy = null;
            user.IsActive = true; 

            return await _userManager.UpdateAsync(user);
        }

        public async Task<UserStatsDto> GetUserStatsAsync()
        {
            var users = _userManager.Users;
            
            var totalUsers = await users.CountAsync(u => !u.IsDeleted);
            var activeUsers = await users.CountAsync(u => u.IsActive && !u.IsDeleted);

            // Fetch counts by roles
            // Note: In a large system, this should be optimized, but for now this works.
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            var storeOwners = await _userManager.GetUsersInRoleAsync("StoreOwner");

            return new UserStatsDto
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                CustomerCount = customers.Count,
                StoreOwnerCount = storeOwners.Count
            };
        }
    }
}
