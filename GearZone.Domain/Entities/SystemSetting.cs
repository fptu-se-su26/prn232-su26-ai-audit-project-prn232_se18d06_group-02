using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities;

public class SystemSetting : Entity<Guid>
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public SettingDataType DataType { get; set; } = SettingDataType.String;
    public string GroupName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
}
