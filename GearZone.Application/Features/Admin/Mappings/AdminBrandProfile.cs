using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings;

public class AdminBrandProfile : Profile
{
    public AdminBrandProfile()
    {
        CreateMap<Brand, AdminBrandDto>()
            .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products.Count));

        CreateMap<CreateBrandDto, Brand>();
        CreateMap<EditBrandDto, Brand>();
    }
}
