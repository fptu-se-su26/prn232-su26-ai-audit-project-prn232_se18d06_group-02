using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IUserRepository
    {
        Task<PagedResult<UserDto>> GetUsersAsync(UserQueryDto query);
        Task<UserDto?> GetUserByIdAsync(string userId);
        
        // Dashboard Methods
        Task<int> GetNewUsersCountAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<ChartDataPoint>> GetUserGrowthAsync(DateTime start, DateTime end, string period, CancellationToken ct = default);
        Task<int> GetTotalUsersCountBeforeAsync(DateTime date, CancellationToken ct = default);
    }
}
