using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Enums;
using Microsoft.Extensions.Caching.Memory;

namespace GearZone.Application.Features.Admin;

public class SystemSettingService : ISystemSettingService
{
    private readonly ISystemSettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private const string CacheKey = "SystemSettings";

    public SystemSettingService(ISystemSettingRepository settingRepository, IUnitOfWork unitOfWork, IMemoryCache cache, IMapper mapper)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _mapper = mapper;
    }

    public async Task<List<SystemSettingDto>> GetAllSettingsAsync()
    {
        if (!_cache.TryGetValue(CacheKey, out List<SystemSettingDto>? cachedSettings) || cachedSettings == null)
        {
            var settings = await _settingRepository.GetAllSettingsAsync();
            cachedSettings = _mapper.Map<List<SystemSettingDto>>(settings);
            

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));

            _cache.Set(CacheKey, cachedSettings, cacheEntryOptions);
        }

        return cachedSettings;
    }

    public async Task<string?> GetSettingValueAsync(string key, string defaultValue = "")
    {
        var allSettings = await GetAllSettingsAsync();
        var setting = allSettings.FirstOrDefault(s => s.Key == key);
        return setting?.Value ?? defaultValue;
    }

    public async Task UpdateSettingsAsync(Dictionary<string, string> settingsData)
    {
        var allSettings = await _settingRepository.GetAllSettingsAsync();
        bool isUpdated = false;

        foreach (var setting in allSettings)
        {
            if (settingsData.TryGetValue(setting.Key, out string? newValue))
            {
                if (setting.DataType == GearZone.Domain.Enums.SettingDataType.Boolean && newValue != null)
                {
                    newValue = newValue.Contains("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
                }

                if (setting.Value != newValue)
                {
                    setting.Value = newValue ?? "";
                    setting.UpdatedAt = DateTime.UtcNow;
                    isUpdated = true;
                }
            }
            else if (setting.DataType == SettingDataType.Boolean)
            {
                if (setting.Value != "false")
                {
                    setting.Value = "false";
                    setting.UpdatedAt = DateTime.UtcNow;
                    isUpdated = true;
                }
            }
        }

        if (isUpdated)
        {
            await _settingRepository.UpdateSettingsAsync(allSettings);
            await _unitOfWork.SaveChangesAsync();
            _cache.Remove(CacheKey);
        }
    }
}
