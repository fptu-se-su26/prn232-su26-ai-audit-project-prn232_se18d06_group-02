using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface ICategoryRepository : IRepository<Category, int>
    {
        Task<PagedResult<Category>> GetPaginatedCategoriesAsync(CategoryQueryDto query);
        Task<List<Category>> GetAllCategoriesListAsync();
        Task<List<CategoryDto>> GetHierarchicalCategoriesAsync(CategoryQueryDto query);
        Task<List<HomeCategoryTileDto>> GetHomeCategoriesBySlugsAsync(IReadOnlyCollection<string> slugs);
    }
}

