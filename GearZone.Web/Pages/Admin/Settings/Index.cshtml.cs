using Microsoft.AspNetCore.Authorization;
using GearZone.Domain.Enums;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Settings;

[Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
{
    private readonly IApiClient _api;

    public IndexModel(IApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public Dictionary<string, string> SettingsData { get; set; } = new();

    public string LastSynced { get; set; } = "";

    public async Task OnGetAsync(CancellationToken ct)
    {
        var data = await _api.GetAsync<AdminSettingsResponseDto>("/api/admin/settings", ct);
        SettingsData = data?.Settings ?? new();
        LastSynced = data?.LastSynced ?? "Never";
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _api.PutAsync("/api/admin/settings", SettingsData, ct);
        if (result.Success)
        {
            TempData["SuccessMessage"] = "Platform settings have been successfully updated!";
        }
        else
        {
            TempData["ErrorMessage"] = result.FirstError ?? "Failed to update settings.";
        }

        return RedirectToPage();
    }
}
