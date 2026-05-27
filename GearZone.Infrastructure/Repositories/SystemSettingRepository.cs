using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories;

public class SystemSettingRepository : Repository<SystemSetting, Guid>, ISystemSettingRepository
{
    private readonly ApplicationDbContext _context;

    public SystemSettingRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<SystemSetting?> GetByKeyAsync(string key)
    {
        return await _context.SystemSettings.FirstOrDefaultAsync(s => s.Key == key);
    }

    public async Task<List<SystemSetting>> GetAllSettingsAsync()
    {
        return await _context.SystemSettings.ToListAsync();
    }

    public async Task UpdateSettingsAsync(List<SystemSetting> settings)
    {
        _context.SystemSettings.UpdateRange(settings);
    }
}
