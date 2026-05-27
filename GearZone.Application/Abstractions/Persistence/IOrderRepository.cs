using System;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using System;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<PagedResult<Order>> GetAdminOrdersAsync(AdminOrderQueryDto queryDto);
        Task<Order?> GetAdminOrderDetailAsync(Guid id);
        Task<AdminOrderStatsDto> GetAdminOrderStatsAsync();
    }
}
