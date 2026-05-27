using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Admin.Categories.Models;

namespace GearZone.Web.Pages.Admin.Categories.Mappings
{
    public class CategoryProfile : Profile
    {
        public CategoryProfile()
        {
            CreateMap<CreateCategoryViewModel, Category>();
            CreateMap<EditCategoryViewModel, EditCategoryDto>();
            CreateMap<EditCategoryDto, Category>();
            CreateMap<CategoryDto, EditCategoryViewModel>();
        }
    }
}
