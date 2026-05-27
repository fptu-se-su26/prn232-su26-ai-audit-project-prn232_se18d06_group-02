using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminOrderService
    {
        Task<PagedResult<AdminOrderDto>> GetOrdersAsync(AdminOrderQueryDto queryDto);
        Task<AdminOrderDetailDto?> GetOrderDetailAsync(Guid id);
        Task<AdminOrderStatsDto> GetOrderStatsAsync();
    }
}
