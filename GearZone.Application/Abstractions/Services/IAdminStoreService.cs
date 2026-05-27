using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services;

public interface IAdminStoreService
{
    Task<PagedResult<StoreApplicationDto>> GetStoreApplicationsAsync(StoreApplicationQueryDto query);
    Task<StoreApplicationDto?> GetStoreApplicationByIdAsync(Guid storeId);
    Task<IEnumerable<GearZone.Domain.Entities.Store>> GetPendingStoresAsync();
    Task<bool> UpdateStoreStatusAsync(Guid storeId, StoreStatus status, string? reason = null);
    Task<bool> RequestInfoAsync(Guid storeId, string note);
    Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync();
    Task<List<StoreApplicationDto>> GetAllStoresAsync();
}
