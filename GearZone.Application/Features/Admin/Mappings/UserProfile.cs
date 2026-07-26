using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Admin.ViewModels;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Role, opt => opt.Ignore());

            CreateMap<CreateUserDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => true));

            // View-model <-> DTO maps (shared by the Razor Admin Users page and the API).
            CreateMap<UserDto, UserViewModel>();
            CreateMap<CreateUserViewModel, CreateUserDto>();
            CreateMap<EditUserViewModel, EditUserDto>();
        }
    }
}
