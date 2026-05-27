using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings;

public class SystemSettingProfile : Profile
{
    public SystemSettingProfile()
    {
        CreateMap<SystemSetting, SystemSettingDto>();
        CreateMap<SystemSettingDto, SystemSetting>();
    }
}
