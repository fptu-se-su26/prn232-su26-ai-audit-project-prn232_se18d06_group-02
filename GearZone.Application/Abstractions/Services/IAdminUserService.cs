using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using Microsoft.AspNetCore.Identity;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminUserService
    {
        Task<PagedResult<UserDto>> GetPaginatedUsersAsync(UserQueryDto query);
        Task<List<string>> GetAllRolesAsync();
        Task<IdentityResult> CreateUserAsync(CreateUserDto dto);
        Task<UserDto?> GetUserByIdAsync(string userId);
        Task<IdentityResult> UpdateUserAsync(EditUserDto dto);
        Task<IdentityResult> SoftDeleteUserAsync(string userId, string deletedByUserId);
        Task<IdentityResult> RestoreUserAsync(string userId);
        Task<UserStatsDto> GetUserStatsAsync();
    }
}
