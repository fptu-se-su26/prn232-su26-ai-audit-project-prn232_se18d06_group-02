using GearZone.Application.Abstractions.Persistence;
using GearZone.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Catalog.DTOs;
using GearZone.Domain.Enums;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Features.Chat.Dtos;

namespace GearZone.Infrastructure.Repositories
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        public ProductRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<ChatProductContextDto?> GetChatProductContextBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            var normalizedSlug = slug.Trim();
            return await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active && p.Slug == normalizedSlug)
                .Select(p => new ChatProductContextDto
                {
                    ProductId = p.Id,
                    StoreId = p.StoreId,
                    StoreName = p.Store.StoreName,
                    StoreSlug = p.Store.Slug,
                    ProductName = p.Name,
                    ProductSlug = p.Slug,
                    ProductImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault(),
                    StoreLogoUrl = p.Store.LogoUrl,
                    Price = p.BasePrice,
                    IsInStock = p.Variants.Any(v => v.StockQuantity > 0)
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CatalogProductDto>> GetCatalogProductsBySlugsAsync(IReadOnlyCollection<string> slugs)
        {
            if (slugs == null || slugs.Count == 0)
            {
                return new List<CatalogProductDto>();
            }

            var normalizedSlugs = slugs
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Select(slug => slug.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedSlugs.Count == 0)
            {
                return new List<CatalogProductDto>();
            }

            return await _context.Products
                .AsNoTracking()
                .Where(p => normalizedSlugs.Contains(p.Slug)
                    && !p.IsDeleted
                    && p.Status == ProductStatus.Active)
                .Select(p => new CatalogProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    CategoryId = p.CategoryId,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty,
                    Rating = p.Reviews.Where(r => !r.IsDeleted).Select(r => (decimal?)r.Rating).Average() ?? 0,
                    ReviewCount = p.Reviews.Count(r => !r.IsDeleted),
                    StoreName = p.Store.StoreName,
                    StoreSlug = p.Store.Slug,
                    StoreLogoUrl = p.Store.LogoUrl ?? string.Empty,
                    IsInStock = p.Variants.Where(v => v.IsActive && !v.IsDeleted).Any(v => v.StockQuantity > 0),
                    DefaultVariantId = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .Select(v => v.Id)
                        .FirstOrDefault(),
                    HighlightTags = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<List<CatalogProductDto>> GetLatestHomeProductsAsync(int take)
        {
            if (take <= 0)
            {
                return new List<CatalogProductDto>();
            }

            return await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active)
                .OrderByDescending(p => p.UpdatedAt ?? p.CreatedAt)
                .ThenByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id)
                .Take(take)
                .Select(p => new CatalogProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    CategoryId = p.CategoryId,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    ImageUrl = p.Images
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? string.Empty,
                    Rating = p.Reviews.Where(r => !r.IsDeleted).Select(r => (decimal?)r.Rating).Average() ?? 0,
                    ReviewCount = p.Reviews.Count(r => !r.IsDeleted),
                    StoreName = p.Store.StoreName,
                    StoreSlug = p.Store.Slug,
                    StoreLogoUrl = p.Store.LogoUrl ?? string.Empty,
                    IsInStock = p.Variants.Where(v => v.IsActive && !v.IsDeleted).Any(v => v.StockQuantity > 0),
                    DefaultVariantId = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .Select(v => v.Id)
                        .FirstOrDefault(),
                    HighlightTags = p.Variants
                        .Where(v => v.IsActive && !v.IsDeleted)
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();
        }

        public async Task<PagedResult<CatalogProductDto>> GetFilteredProductsAsync(ProductFilterDto filter)
        {
            // Use AsNoTracking for read-only query performance
            // REMOVED: All .Include() calls to avoid Cartesian explosion and over-fetching
            var query = _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active);

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var searchTerm = filter.Search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Brand.Name.ToLower().Contains(searchTerm) ||
                    p.Category.Name.ToLower().Contains(searchTerm) ||
                    p.Category.Slug.ToLower().Contains(searchTerm) ||
                    (p.Category.Parent != null &&
                     (p.Category.Parent.Name.ToLower().Contains(searchTerm) ||
                      p.Category.Parent.Slug.ToLower().Contains(searchTerm))));
            }

            if (filter.StoreId.HasValue && filter.StoreId.Value != Guid.Empty)
            {
                query = query.Where(p => p.StoreId == filter.StoreId.Value);
            }

            var categorySlugs = filter.CategorySlugs?
                .Where(slug => !string.IsNullOrWhiteSpace(slug))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (categorySlugs is { Count: > 0 })
            {
                var categoryIds = await _context.Categories
                    .Where(c =>
                        categorySlugs.Contains(c.Slug) ||
                        (c.Parent != null && categorySlugs.Contains(c.Parent.Slug)))
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }
            else if (!string.IsNullOrEmpty(filter.CategorySlug))
            {
                // Collect IDs: the matched category itself + all its direct children
                var categoryIds = await _context.Categories
                    .Where(c =>
                        c.Slug == filter.CategorySlug ||
                        (c.Parent != null && c.Parent.Slug == filter.CategorySlug))
                    .Select(c => c.Id)
                    .ToListAsync();

                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            if (filter.BrandSlugs != null && filter.BrandSlugs.Any())
            {
                query = query.Where(p => filter.BrandSlugs.Contains(p.Brand.Slug));
            }

            if (filter.InStockOnly == true)
            {
                query = query.Where(p => p.Variants.Any(v => v.StockQuantity > 0));
            }

            if (filter.Attributes != null && filter.Attributes.Any())
            {
                foreach (var attr in filter.Attributes)
                {
                    var attrName = attr.Key;
                    var attrValues = attr.Value;
                    if (attrValues != null && attrValues.Any())
                    {
                        query = query.Where(p => p.Variants.Any(v => 
                            v.AttributeValues.Any(av => 
                                av.CategoryAttribute.Name == attrName && 
                                attrValues.Contains(av.CategoryAttributeOption.Value)
                            )
                        ));
                    }
                }
            }

            var now = DateTime.UtcNow;
            var pricedQuery = query.Select(product => new
            {
                Product = product,
                EffectivePrice = product.PromotionProducts
                    .Where(link =>
                        link.Campaign.IsEnabled &&
                        link.Campaign.StartAt <= now &&
                        link.Campaign.EndAt > now &&
                        link.Campaign.ReservedQuantity + link.Campaign.RedeemedQuantity <
                            link.Campaign.TotalQuantityLimit)
                    .Select(link => (decimal?)(
                        link.Campaign.DiscountType == DiscountType.Percent
                            ? product.BasePrice -
                              product.BasePrice * link.Campaign.DiscountValue / 100m
                            : product.BasePrice > link.Campaign.DiscountValue
                                ? product.BasePrice - link.Campaign.DiscountValue
                                : 0m))
                    .FirstOrDefault() ?? product.BasePrice
            });

            if (filter.MinPrice.HasValue)
            {
                pricedQuery = pricedQuery.Where(x =>
                    x.EffectivePrice >= filter.MinPrice.Value);
            }

            if (filter.MaxPrice.HasValue)
            {
                pricedQuery = pricedQuery.Where(x =>
                    x.EffectivePrice <= filter.MaxPrice.Value);
            }

            var totalCount = await pricedQuery.CountAsync();

            // Price filters and ordering use the same effective-price expression
            // as the promotion pricing service. DTO mapping retains the original
            // price so Application applies promotion metadata exactly once.
            pricedQuery = (filter.SortBy?.ToLower()) switch
            {
                "newest" => pricedQuery
                    .OrderByDescending(x => x.Product.CreatedAt)
                    .ThenBy(x => x.Product.Id),
                "price_asc" => pricedQuery
                    .OrderBy(x => x.EffectivePrice)
                    .ThenBy(x => x.Product.Id),
                "price_desc" => pricedQuery
                    .OrderByDescending(x => x.EffectivePrice)
                    .ThenBy(x => x.Product.Id),
                _ => pricedQuery
                    .OrderByDescending(x => x.Product.SoldCount)
                    .ThenBy(x => x.Product.Id)
            };

            // PRODUCTION OPTIMIZED PROJECTION
            // Maps directly to DTO to fetch ONLY required columns and avoid Cartesian joins
            var items = await pricedQuery
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new CatalogProductDto
                {
                    Id = x.Product.Id,
                    Name = x.Product.Name,
                    Slug = x.Product.Slug,
                    CategoryId = x.Product.CategoryId,
                    BrandName = x.Product.Brand.Name,
                    BasePrice = x.Product.BasePrice,
                    ImageUrl = x.Product.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                               ?? x.Product.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? "",
                    Rating = x.Product.Reviews.Where(r => !r.IsDeleted).Select(r => (decimal?)r.Rating).Average() ?? 0,
                    ReviewCount = x.Product.Reviews.Count(r => !r.IsDeleted),
                    StoreName = x.Product.Store.StoreName,
                    StoreSlug = x.Product.Store.Slug,
                    StoreLogoUrl = x.Product.Store.LogoUrl,
                    IsInStock = x.Product.Variants.Any(v => v.StockQuantity > 0),
                    DefaultVariantId = x.Product.Variants
                        .OrderByDescending(v => v.IsActive)
                        .Select(v => v.Id)
                        .FirstOrDefault(),
                    HighlightTags = x.Product.Variants
                        .SelectMany(v => v.AttributeValues)
                        .Select(av => av.CategoryAttributeOption.Value)
                        .Distinct()
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();

            return new PagedResult<CatalogProductDto>(items, totalCount, filter.PageNumber, filter.PageSize);
        }

        public async Task<List<ProductSuggestionDto>> GetProductSuggestionsAsync(string query, int limit = 5)
        {
            if (string.IsNullOrWhiteSpace(query)) return new List<ProductSuggestionDto>();

            var searchTerm = query.Trim().ToLower();

            return await _context.Products
                .AsNoTracking()
                .Where(p => !p.IsDeleted && p.Status == ProductStatus.Active &&
                           (p.Name.ToLower().Contains(searchTerm) || p.Brand.Name.ToLower().Contains(searchTerm)))
                .OrderByDescending(p => p.SoldCount)
                .Take(limit)
                .Select(p => new ProductSuggestionDto
                {
                    Name = p.Name,
                    Slug = p.Slug,
                    BrandName = p.Brand.Name,
                    Price = p.BasePrice,
                    ImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() 
                               ?? p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? ""
                })
                .ToListAsync();
        }

        public async Task<PagedResult<Product>> GetAdminProductsAsync(AdminProductQueryDto queryDto)
        {
            var query = _context.Products
                .Include(p => p.Store)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                .Include(p => p.Images)
                .AsQueryable();

            // Search across Name, SKU (any variant) and StoreName simultaneously
            if (!string.IsNullOrEmpty(queryDto.SearchTerm))
            {
                var searchTerm = queryDto.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchTerm) ||
                    p.Variants.Any(v => v.Sku.ToLower().Contains(searchTerm)) ||
                    p.Store.StoreName.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrEmpty(queryDto.Status))
            {
                if (Enum.TryParse<ProductStatus>(queryDto.Status, true, out var status))
                {
                    query = query.Where(p => p.Status == status);
                }
            }

            if (queryDto.CategoryId.HasValue && queryDto.CategoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == queryDto.CategoryId.Value);
            }

            if (queryDto.BrandId.HasValue && queryDto.BrandId.Value > 0)
            {
                query = query.Where(p => p.BrandId == queryDto.BrandId.Value);
            }

            if (queryDto.StoreId.HasValue && queryDto.StoreId.Value != Guid.Empty)
            {
                query = query.Where(p => p.StoreId == queryDto.StoreId.Value);
            }

            if (queryDto.MinPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice >= queryDto.MinPrice.Value);
            }

            if (queryDto.MaxPrice.HasValue)
            {
                query = query.Where(p => p.BasePrice <= queryDto.MaxPrice.Value);
            }

            if (queryDto.StartDate.HasValue)
            {
                query = query.Where(p => p.CreatedAt >= queryDto.StartDate.Value);
            }

            if (queryDto.EndDate.HasValue)
            {
                var endOfDay = queryDto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(p => p.CreatedAt <= endOfDay);
            }

            if (queryDto.OutOfStock)
            {
                query = query.Where(p => !p.Variants.Any(v => v.StockQuantity > 0));
            }

            // Filter by selected attribute option IDs (products must have at least one matching variant)
            if (queryDto.AttributeOptionIds != null && queryDto.AttributeOptionIds.Any())
            {
                query = query.Where(p =>
                    p.Variants.Any(v =>
                        v.AttributeValues.Any(av =>
                            queryDto.AttributeOptionIds.Contains(av.CategoryAttributeOptionId))));
            }

            // Dynamic sort: SortBy controls column, SortDirection controls asc/desc
            // If no SortBy, default to newest first
            var sortBy = (queryDto.SortBy ?? "").ToLower();
            var isAsc = (queryDto.SortDirection ?? "").ToLower() == "asc";

            query = sortBy switch
            {
                "stock" => isAsc
                    ? query.OrderBy(p => p.Variants.Sum(v => (int?)v.StockQuantity) ?? 0).ThenBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.Variants.Sum(v => (int?)v.StockQuantity) ?? 0).ThenByDescending(p => p.CreatedAt),
                "price" => isAsc
                    ? query.OrderBy(p => p.BasePrice).ThenBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.BasePrice).ThenByDescending(p => p.CreatedAt),
                "createdat" => isAsc
                    ? query.OrderBy(p => p.CreatedAt)
                    : query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt) // default
            };

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((queryDto.PageNumber - 1) * queryDto.PageSize)
                .Take(queryDto.PageSize)
                .ToListAsync();

            return new PagedResult<Product>(items, totalCount, queryDto.PageNumber, queryDto.PageSize);
        }

        public async Task<AdminProductStatsDto> GetAdminProductStatsAsync()
        {
            var totalProducts = await _context.Products.CountAsync();
            var activeProducts = await _context.Products.CountAsync(p => p.Status == ProductStatus.Active);
            var pendingApproval = await _context.Products.CountAsync(p => p.Status == ProductStatus.Pending);
            
            // Out of stock means no variant has stock > 0
            var outOfStock = await _context.Products.CountAsync(p => !p.Variants.Any(v => v.StockQuantity > 0));

            return new AdminProductStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                PendingApproval = pendingApproval,
                OutOfStock = outOfStock
            };
        }

        public async Task<Product?> GetAdminProductDetailAsync(Guid id)
        {
            return await _context.Products
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.Store)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
    }
}
