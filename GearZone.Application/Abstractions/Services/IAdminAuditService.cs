using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services;

public interface IAdminAuditService
{
    Task<PagedResult<AdminAuditLogListItemDto>> GetLogsAsync(AdminAuditQueryDto query, CancellationToken ct = default);
    Task<AdminAuditDetailDto?> GetDetailAsync(Guid id, CancellationToken ct = default);
    Task<AdminAuditSummaryDto> GetSummaryAsync(AdminAuditQueryDto query, CancellationToken ct = default);
    Task<AdminAuditFilterOptionsDto> GetFilterOptionsAsync(CancellationToken ct = default);
    Task<AdminAuditFileDto> ExportCsvAsync(AdminAuditQueryDto query, CancellationToken ct = default);
}
