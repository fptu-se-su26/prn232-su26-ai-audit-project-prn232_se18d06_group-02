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

        /// <summary>Returns the subset of the given SKUs that already exist anywhere in the system.</summary>
        Task<HashSet<string>> GetExistingSkusAsync(IEnumerable<string> skus);

        /// <summary>Returns the subset of the given slugs that already exist (not deleted) in the store.</summary>
        Task<HashSet<string>> GetExistingSlugsAsync(Guid storeId, IEnumerable<string> slugs);

        /// <summary>Maps each given SKU that belongs to this store to its variant reference (id, product, stock).</summary>
        Task<Dictionary<string, StoreVariantRefDto>> GetStoreVariantsBySkuAsync(Guid storeId, IEnumerable<string> skus);

        /// <summary>Maps each given slug that exists (not deleted) in this store to its product id.</summary>
        Task<Dictionary<string, Guid>> GetStoreProductIdsBySlugAsync(Guid storeId, IEnumerable<string> slugs);

        /// <summary>Increases an existing variant's stock by the given amount and logs an inventory transaction.</summary>
        Task RestockVariantAsync(Guid variantId, int addQuantity, string userId);

        /// <summary>Adds a new variant to an existing product (with an initial-stock inventory transaction).</summary>
        Task AddVariantToProductAsync(Guid productId, string variantName, string sku, decimal price, int stockQuantity, string userId);
    }
}



