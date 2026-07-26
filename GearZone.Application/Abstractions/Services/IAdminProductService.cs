using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;

namespace GearZone.Application.Abstractions.Services
{
    public interface IAdminProductService
    {
        Task<PagedResult<AdminProductDto>> GetProductsAsync(AdminProductQueryDto queryDto);
        Task<AdminCsvFileDto> ExportProductsCsvAsync(AdminProductQueryDto queryDto, CancellationToken ct = default);
        Task<AdminProductStatsDto> GetProductStatsAsync();
        Task<AdminProductDetailDto?> GetProductDetailAsync(Guid id);
        Task<bool> DeleteProductAsync(Guid id, string reason);
        Task<bool> BulkUpdateStatusAsync(List<Guid> productIds, ProductStatus status, string? reason = null);
    }
}
