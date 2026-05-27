using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IStoreRepository : IRepository<Store, Guid>
    {
        Task<PagedResult<Store>> GetStoreApplicationsAsync(StoreApplicationQueryDto query);
        Task<Store?> GetStoreApplicationByIdAsync(Guid storeId);
        Task<Store?> GetStoreByOwnerIdAsync(string userId);
        Task<StoreApplicationStatsDto> GetStoreApplicationStatsAsync();
        Task<Store?> GetBySlugAsync(string slug);
        
        // Dashboard Methods
        Task<int> GetActiveStoresCountAsync(CancellationToken ct = default);
        Task<int> GetNewStoresCountAsync(DateTime start, DateTime end, CancellationToken ct = default);
        Task<List<HomeStoreCardDto>> GetHomeStoresBySlugsAsync(IReadOnlyCollection<string> slugs);
    }
}
