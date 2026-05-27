using Microsoft.AspNetCore.Authorization;
using GearZone.Application.Features.Admin;
using GearZone.Application.Abstractions.Services;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GearZone.Web.Pages.Admin.Settings;

[Authorize(Roles = "Super Admin")]
    public class IndexModel : PageModel
{
    private readonly ISystemSettingService _settingService;

    public IndexModel(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    [BindProperty]
    public Dictionary<string, string> SettingsData { get; set; } = new();

    public string LastSynced { get; set; } = "";

    public async Task OnGetAsync()
    {
        var settings = await _settingService.GetAllSettingsAsync();
        SettingsData = settings.ToDictionary(s => s.Key, s => s.Value);

        var latestUpdate = settings.Max(s => s.UpdatedAt);
        LastSynced = latestUpdate?.ToLocalTime().ToString("f") ?? "Never";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _settingService.UpdateSettingsAsync(SettingsData);
            TempData["SuccessMessage"] = "Platform settings have been successfully updated!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Failed to update settings: {ex.Message}";
        }

        return RedirectToPage();
    }
}
