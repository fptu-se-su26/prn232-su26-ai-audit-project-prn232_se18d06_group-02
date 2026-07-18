using GearZone.Api.Auditing;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/audit-logs")]
[ApiController]
public sealed class AuditLogsController : BaseApiController
{
    private readonly IAdminAuditService _service;

    public AuditLogsController(IAdminAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] AdminAuditQueryDto query, CancellationToken ct)
    {
        try { return OkResponse(await _service.GetLogsAsync(query, ct)); }
        catch (ArgumentException ex) { return FailResponse(ex.Message); }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Detail(Guid id, CancellationToken ct)
    {
        var detail = await _service.GetDetailAsync(id, ct);
        return detail is null ? FailResponse("Audit log not found.", 404) : OkResponse(detail);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] AdminAuditQueryDto query, CancellationToken ct)
    {
        try { return OkResponse(await _service.GetSummaryAsync(query, ct)); }
        catch (ArgumentException ex) { return FailResponse(ex.Message); }
    }

    [HttpGet("filter-options")]
    public async Task<IActionResult> FilterOptions(CancellationToken ct) =>
        OkResponse(await _service.GetFilterOptionsAsync(ct));

    [HttpGet("export")]
    [AdminAuditAction(
        AdminAuditActions.AuditLogExported,
        AdminAuditModules.Audit,
        AdminAuditRiskLevel.Medium,
        Description = "Exported filtered admin audit logs")]
    public async Task<IActionResult> Export([FromQuery] AdminAuditQueryDto query, [FromQuery] string format = "csv", CancellationToken ct = default)
    {
        if (!string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
            return FailResponse("Audit logs v1 supports CSV export only.");

        try
        {
            var file = await _service.ExportCsvAsync(query, ct);
            return File(file.Content, file.ContentType, file.FileName);
        }
        catch (ArgumentException ex)
        {
            return FailResponse(ex.Message);
        }
    }
}
