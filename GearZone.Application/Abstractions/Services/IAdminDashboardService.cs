using GearZone.Application.Features.Admin.Dtos;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardDto> GetDashboardDataAsync(DashboardQuery query);
    }
}
