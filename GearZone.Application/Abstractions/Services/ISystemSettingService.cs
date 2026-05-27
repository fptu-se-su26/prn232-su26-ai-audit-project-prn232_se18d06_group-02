using GearZone.Application.Features.Admin.Dtos;

namespace GearZone.Application.Abstractions.Services;

public interface ISystemSettingService
{
    Task<string?> GetSettingValueAsync(string key, string defaultValue = "");
    Task<List<SystemSettingDto>> GetAllSettingsAsync();
    Task UpdateSettingsAsync(Dictionary<string, string> settingsData);
}
