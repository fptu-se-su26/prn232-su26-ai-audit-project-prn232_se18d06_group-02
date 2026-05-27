using System;
using System.Threading.Tasks;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<PagedResult<Product>> GetAdminProductsAsync(AdminProductQueryDto query);
        Task<AdminProductStatsDto> GetAdminProductStatsAsync();
        Task<Product?> GetAdminProductDetailAsync(Guid id);
        Task<ChatProductContextDto?> GetChatProductContextBySlugAsync(string slug);
        Task<List<CatalogProductDto>> GetCatalogProductsBySlugsAsync(IReadOnlyCollection<string> slugs);
        Task<List<CatalogProductDto>> GetLatestHomeProductsAsync(int take);
        Task<PagedResult<CatalogProductDto>> GetFilteredProductsAsync(ProductFilterDto filter);
        Task<List<ProductSuggestionDto>> GetProductSuggestionsAsync(string query, int limit = 5);
    }
}
