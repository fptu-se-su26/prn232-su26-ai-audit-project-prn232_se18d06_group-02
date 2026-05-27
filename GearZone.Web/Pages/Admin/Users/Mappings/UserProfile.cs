using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Web.Pages.Admin.Users.Models;

namespace GearZone.Web.Pages.Admin.Users.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserDto, UserViewModel>();
            CreateMap<CreateUserViewModel, CreateUserDto>();
            CreateMap<EditUserViewModel, EditUserDto>();
        }
    }
}
