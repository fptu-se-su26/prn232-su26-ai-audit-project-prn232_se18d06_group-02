using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : null));

            CreateMap<CategoryAttributeOption, CategoryAttributeOptionDto>();

            CreateMap<CategoryAttribute, CategoryAttributeDto>()
                .ForMember(dest => dest.Options, opt => opt.MapFrom(src => src.Options));

            // Maps DTO values into an existing tracked Category entity (Id is ignored to preserve the PK)
            CreateMap<EditCategoryDto, Category>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());
        }
    }
}
