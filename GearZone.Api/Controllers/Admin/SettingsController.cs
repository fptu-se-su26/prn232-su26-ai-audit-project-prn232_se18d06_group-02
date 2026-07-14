using GearZone.Application.Abstractions.Services;
using GearZone.Api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers.Admin;

[Authorize(Roles = "Super Admin")]
[Route("api/admin/settings")]
[ApiController]
public class SettingsController : BaseApiController
{
    private readonly ISystemSettingService _settingService;

    public SettingsController(ISystemSettingService settingService)
    {
        _settingService = settingService;
    }

    // GET /api/admin/settings
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var settings = await _settingService.GetAllSettingsAsync();
        var dict = settings.ToDictionary(s => s.Key, s => s.Value);
        var lastSynced = settings.Max(s => s.UpdatedAt)?.ToLocalTime().ToString("f") ?? "Never";
        return OkResponse(new { settings = dict, lastSynced });
    }

    // PUT /api/admin/settings
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] Dictionary<string, string> settings)
    {
        await _settingService.UpdateSettingsAsync(settings);
        return OkResponse("Settings updated.");
    }
}
