using AutoMapper;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Admin
{
    public class AdminCategoryService : IAdminCategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public AdminCategoryService(
            ICategoryRepository categoryRepository,
            ICategoryAttributeRepository categoryAttributeRepository,
            IMapper mapper,
            IUnitOfWork unitOfWork)
        {
            _categoryRepository = categoryRepository;
            _categoryAttributeRepository = categoryAttributeRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<CategoryDto>> GetPaginatedCategoriesAsync(CategoryQueryDto query)
        {
            var pagedCategories = await _categoryRepository.GetPaginatedCategoriesAsync(query);
            
            var dtos = _mapper.Map<List<CategoryDto>>(pagedCategories.Items);

            return new PagedResult<CategoryDto>(dtos, pagedCategories.TotalCount, query.PageNumber, query.PageSize);
        }

        public async Task<bool> CreateCategoryAsync(Category category)
        {
            try
            {
                await _categoryRepository.AddAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCategoryAsync(EditCategoryDto dto)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(dto.Id);
                if (category == null) return false;

                _mapper.Map(dto, category);

                await _categoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> SoftDeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null) return false;

                category.IsDeleted = true;
                category.IsActive = false; // Optionally mark as inactive

                await _categoryRepository.UpdateAsync(category);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null) return null;

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<List<CategoryDto>> GetAllCategoriesListAsync()
        {
            var categories = await _categoryRepository.GetAllCategoriesListAsync();
            return _mapper.Map<List<CategoryDto>>(categories);
        }

        public async Task<List<CategoryDto>> GetHierarchicalCategoriesAsync(CategoryQueryDto query)
        {
            return await _categoryRepository.GetHierarchicalCategoriesAsync(query);
        }

        public async Task<List<CategoryAttributeDto>> GetAttributesByCategoryIdAsync(int categoryId)
        {
            var attrs = await _categoryAttributeRepository.GetByCategoryIdAsync(categoryId);
            return _mapper.Map<List<CategoryAttributeDto>>(attrs);
        }

        public async Task<bool> SaveCategoryAttributesAsync(SaveCategoryAttributesRequest request)
        {
            try
            {
                // Delete existing attributes for this category (cascade deletes options)
                var existing = await _categoryAttributeRepository.GetByCategoryIdAsync(request.CategoryId);
                await _categoryAttributeRepository.DeleteRangeAsync(existing);

                // Re-insert from request
                int attrOrder = 0;
                foreach (var attrDto in request.Attributes)
                {
                    var attr = new CategoryAttribute
                    {
                        CategoryId = request.CategoryId,
                        Name = attrDto.Name,
                        FilterType = attrDto.FilterType,
                        IsFilterable = attrDto.IsFilterable,
                        DisplayOrder = attrOrder++,
                        Options = attrDto.Options.Select((o, i) => new CategoryAttributeOption
                        {
                            Value = o.Value,
                            DisplayOrder = i
                        }).ToList()
                    };
                    await _categoryAttributeRepository.AddAsync(attr);
                }

                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

