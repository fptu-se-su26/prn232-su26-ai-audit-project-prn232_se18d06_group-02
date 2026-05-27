using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GearZone.Application.Abstractions.Services
{
    public interface ISellerProductService
    {
        Task<List<SellerProductListDto>> GetProductsByStoreAsync(Guid storeId);
        Task<SellerProductDetailDto?> GetProductByIdAsync(Guid productId, Guid storeId);
        Task<Guid> CreateProductAsync(CreateProductDto dto, Guid storeId, string userId);
        Task<UpdateProductDto?> GetProductForEditAsync(Guid productId, Guid storeId);
        Task UpdateProductAsync(Guid productId, UpdateProductDto dto, Guid storeId, string userId);
        Task<List<Category>> GetCategoriesAsync();
        Task<List<Brand>> GetBrandsAsync();
        Task<List<CategoryAttributeDto>> GetCategoryAttributesAsync(int categoryId);
        Task<List<ProductSpecDto>> GetCategoryProductSpecsAsync(int categoryId);
        Task<int> CreateCategoryProductSpecificationAsync(int categoryId, string name, string? unit = null, string? valueType = null);
        Task ToggleProductStatusAsync(Guid productId, Guid storeId);
        Task<int> CreateBrandByNameAsync(string name);
        Task<int> CreateCategoryByNameAsync(string name);
    }
}



