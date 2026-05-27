using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GearZone.Infrastructure.Repositories
{
    public class CategoryRepository : Repository<Category, int>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PagedResult<Category>> GetPaginatedCategoriesAsync(CategoryQueryDto query)
        {
            var dbQuery = _dbSet
                .Include(c => c.Parent)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                dbQuery = dbQuery.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Slug.ToLower().Contains(search));
            }

            if (query.IsActive.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.IsActive == query.IsActive.Value);
            }

            if (query.ParentId.HasValue)
            {
                dbQuery = dbQuery.Where(c => c.ParentId == query.ParentId.Value);
            }

            var totalCount = await dbQuery.CountAsync();

            var categories = await dbQuery
                .OrderByDescending(c => c.Id)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Category>
            {
                Items = categories,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<List<Category>> GetAllCategoriesListAsync()
        {
            return await _dbSet.OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<List<CategoryDto>> GetHierarchicalCategoriesAsync(CategoryQueryDto query)
        {
            // Load root categories with their children and product counts
            var rootQuery = _dbSet
                .Include(c => c.Children)
                    .ThenInclude(child => child.Products)
                .Include(c => c.Products)
                .Where(c => c.ParentId == null);

            // Apply search filter (to roots or children matching)
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                rootQuery = rootQuery.Where(c =>
                    c.Name.ToLower().Contains(search) ||
                    c.Slug.ToLower().Contains(search) ||
                    c.Children.Any(ch => ch.Name.ToLower().Contains(search) || ch.Slug.ToLower().Contains(search)));
            }

            // Apply status filter
            if (query.IsActive.HasValue)
            {
                rootQuery = rootQuery.Where(c => c.IsActive == query.IsActive.Value ||
                    c.Children.Any(ch => ch.IsActive == query.IsActive.Value));
            }

            var roots = await rootQuery
                .OrderBy(c => c.Name)
                .ToListAsync();

            var result = roots.Select(root =>
            {
                var rootDto = MapToCategoryDto(root, null, null);

                rootDto.Children = root.Children
                    .OrderBy(ch => ch.Name)
                    .Select(child => MapToCategoryDto(child, root.Id, root.Name))
                    .ToList();

                return rootDto;
            }).ToList();

            return result;
        }

        public async Task<List<HomeCategoryTileDto>> GetHomeCategoriesBySlugsAsync(IReadOnlyCollection<string> slugs)
        {
            if (slugs == null || slugs.Count == 0)
            {
                return new List<HomeCategoryTileDto>();
            }

            var normalizedSlugs = slugs
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Select(slug => slug.Trim())
                .Distinct()
                .ToList();

            return await _dbSet
                .AsNoTracking()
                .Where(c => normalizedSlugs.Contains(c.Slug) && c.IsActive && !c.IsDeleted)
                .Select(c => new HomeCategoryTileDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug,
                    ProductCount = c.Products.Count(p => !p.IsDeleted && p.Status == GearZone.Domain.Enums.ProductStatus.Active)
                })
                .ToListAsync();
        }

        private static CategoryDto MapToCategoryDto(Category c, int? parentId, string? parentName)
        {
            return new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                IsActive = c.IsActive,
                IsDeleted = c.IsDeleted,
                ParentId = parentId ?? c.ParentId,
                ParentName = parentName,
                ProductCount = c.Products.Count,
                Revenue = 0 // Revenue can be extended separately via OrderItem queries if needed
            };
        }
    }
}

