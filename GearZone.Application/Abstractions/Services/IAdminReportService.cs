using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services;

public interface IAdminReportService
{
    Task<AdminOverviewReportDto> GetOverviewAsync(AdminReportQueryDto query, CancellationToken ct = default);
    Task<AdminOrderReportDto> GetOrdersAsync(AdminReportQueryDto query, CancellationToken ct = default);
    Task<AdminSellerReportDto> GetSellersAsync(AdminReportQueryDto query, bool exportAll = false, CancellationToken ct = default);
    Task<object> GetInsightSnapshotAsync(string reportType, AdminReportQueryDto query, CancellationToken ct = default);
}

public interface IAdminReportExportService
{
    Task<AdminReportFileDto> ExportAsync(
        string reportType,
        string format,
        AdminReportQueryDto query,
        CancellationToken ct = default);
}

public interface IAdminAiInsightService
{
    Task<AdminAiInsightDto?> GetCachedAsync(
        string reportType,
        AdminReportQueryDto query,
        CancellationToken ct = default);

    Task<AdminAiInsightDto> GenerateAsync(
        string reportType,
        AdminReportQueryDto query,
        bool forceRefresh,
        CancellationToken ct = default);
}

