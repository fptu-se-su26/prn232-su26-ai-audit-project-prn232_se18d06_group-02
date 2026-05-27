using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ISystemSettingRepository : IRepository<SystemSetting, Guid>
    {
        Task<SystemSetting?> GetByKeyAsync(string key);
        Task<List<SystemSetting>> GetAllSettingsAsync();
        Task UpdateSettingsAsync(List<SystemSetting> settings);
    }
}
