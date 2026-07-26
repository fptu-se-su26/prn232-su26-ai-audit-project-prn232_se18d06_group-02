using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.AuditLogs;

[Authorize(Roles = "Super Admin")]
public sealed class DetailModel : PageModel
{
    private readonly IApiClient _api;

    public DetailModel(IApiClient api)
    {
        _api = api;
    }

    public AdminAuditDetailDto Audit { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        try
        {
            var item = await _api.GetAsync<AdminAuditDetailDto>($"/api/admin/audit-logs/{id}", ct);
            if (item is null) return NotFound();
            Audit = item;
            return Page();
        }
        catch (HttpRequestException)
        {
            return RedirectToPage("/Admin/AuditLogs/Index");
        }
    }
}
