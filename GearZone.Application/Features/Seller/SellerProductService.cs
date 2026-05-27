using GearZone.Application.Abstractions.External;
using GearZone.Application.Common.ProductSpecifications;
using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Seller.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Seller
{
    public class SellerProductService : ISellerProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IBrandRepository _brandRepository;
        private readonly IProductImageRepository _productImageRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IInventoryTransactionRepository _inventoryRepository;
        private readonly IVariantAttributeValueRepository _attributeValueRepository;
        private readonly IProductAttributeValueRepository _productAttributeValueRepository;
        private readonly ICategoryAttributeRepository _categoryAttributeRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUnitOfWork _unitOfWork;

        public SellerProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IBrandRepository brandRepository,
            IProductImageRepository productImageRepository,
            IProductVariantRepository productVariantRepository,
            IInventoryTransactionRepository inventoryRepository,
            IVariantAttributeValueRepository attributeValueRepository,
            IProductAttributeValueRepository productAttributeValueRepository,
            ICategoryAttributeRepository categoryAttributeRepository,
            IFileStorageService fileStorageService,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _brandRepository = brandRepository;
            _productImageRepository = productImageRepository;
            _productVariantRepository = productVariantRepository;
            _inventoryRepository = inventoryRepository;
            _attributeValueRepository = attributeValueRepository;
            _productAttributeValueRepository = productAttributeValueRepository;
            _categoryAttributeRepository = categoryAttributeRepository;
            _fileStorageService = fileStorageService;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<SellerProductListDto>> GetProductsByStoreAsync(Guid storeId)
        {
            return await _productRepository.Query()
                .Where(p => p.StoreId == storeId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new SellerProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Slug = p.Slug,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    BrandId = p.BrandId,
                    BrandName = p.Brand.Name,
                    BasePrice = p.BasePrice,
                    TotalStock = p.Variants.Sum(v => v.StockQuantity),
                    Status = p.Status.ToString(),
                    CreatedAt = p.CreatedAt,
                    PrimaryImageUrl = p.Images.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault()
                })
                .ToListAsync();
        }

        public async Task<SellerProductDetailDto?> GetProductByIdAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttributeOption)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) return null;

            var legacySpecs = ParseSpecsJson(product.SpecsJson);
            var specs = BuildProductSpecificationDictionary(product.Category?.Slug, product.AttributeValues, legacySpecs);

            return new SellerProductDetailDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                CategoryName = product.Category?.Name ?? string.Empty,
                BrandName = product.Brand?.Name ?? string.Empty,
                BasePrice = product.BasePrice,
                SoldCount = product.SoldCount,
                Status = product.Status.ToString(),
                CreatedAt = product.CreatedAt,
                Specifications = specs,
                ImageUrls = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                Variants = product.Variants.Select(v => new ProductVariantDto
                {
                    VariantName = v.VariantName ?? "",
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.AttributeValues.Select(av => new AttributeSelectionDto
                    {
                        AttributeId = av.CategoryAttributeId,
                        OptionId = av.CategoryAttributeOptionId,
                        AttributeName = av.CategoryAttribute?.Name ?? "",
                        OptionValue = av.CategoryAttributeOption?.Value ?? ""
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<Guid> CreateProductAsync(CreateProductDto dto, Guid storeId, string userId)
        {
            var slug = string.IsNullOrEmpty(dto.Slug) ? dto.Name.ToLower().Replace(" ", "-") : dto.Slug;
            await PrepareGeneratedVariantIdentityAsync(dto.Name, dto.CategoryId, dto.Variants);

            var existingProduct = await _productRepository.Query()
                .AnyAsync(p => p.StoreId == storeId && p.Slug == slug && !p.IsDeleted);

            if (existingProduct)
            {
                throw new InvalidOperationException($"A product with the slug '{slug}' already exists in your store.");
            }

            if (dto.Variants != null && dto.Variants.Any())
            {
                var skus = dto.Variants.Select(v => v.Sku).ToList();
                var existingSkus = await _productVariantRepository.Query()
                    .Where(v => skus.Contains(v.Sku))
                    .Select(v => v.Sku)
                    .ToListAsync();

                if (existingSkus.Any())
                {
                    throw new InvalidOperationException($"The following SKUs already exist in the system: {string.Join(", ", existingSkus)}");
                }
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                CategoryId = dto.CategoryId,
                BrandId = dto.BrandId,
                Name = dto.Name,
                Slug = slug,
                Description = dto.Description,
                BasePrice = dto.BasePrice,
                SpecsJson = BuildSpecsJson(dto.Specifications),
                Status = dto.IsDraft ? ProductStatus.Draft : ProductStatus.Pending,
                SoldCount = 0,
                CreatedAt = DateTime.UtcNow,
            };

            await _productRepository.AddAsync(product);

            const int maxImages = 5;
            if (dto.Images != null && dto.Images.Any())
            {
                var imagesToUpload = dto.Images.Take(maxImages).ToList();
                var imageUrls = await _fileStorageService.UploadAsync(imagesToUpload);
                int sortOrder = 0;
                foreach (var imageUrl in imageUrls)
                {
                    await _productImageRepository.AddAsync(new ProductImage
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        ImageUrl = imageUrl,
                        IsPrimary = sortOrder == 0,
                        SortOrder = sortOrder++
                    });
                }
            }

            foreach (var spec in dto.Specifications.Where(IsValidProductSpecification))
            {
                await _productAttributeValueRepository.AddAsync(new ProductAttributeValue
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CategoryAttributeId = spec.AttributeId,
                    Value = spec.Value.Trim()
                });
            }

            foreach (var vDto in dto.Variants ?? Enumerable.Empty<ProductVariantDto>())
            {
                var variant = new ProductVariant
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    Sku = vDto.Sku,
                    VariantName = vDto.VariantName,
                    Price = vDto.Price,
                    StockQuantity = vDto.StockQuantity,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _productVariantRepository.AddAsync(variant);

                if (variant.StockQuantity > 0)
                {
                    await _inventoryRepository.AddAsync(new InventoryTransaction
                    {
                        Id = Guid.NewGuid(),
                        VariantId = variant.Id,
                        Type = "StockIn",
                        QuantityChange = variant.StockQuantity,
                        Reason = "Initial stock upload",
                        CreatedAt = DateTime.UtcNow,
                        CreatedByUserId = userId
                    });
                }

                foreach (var attr in vDto.Attributes)
                {
                    await _attributeValueRepository.AddAsync(new VariantAttributeValue
                    {
                        Id = Guid.NewGuid(),
                        VariantId = variant.Id,
                        CategoryAttributeId = attr.AttributeId,
                        CategoryAttributeOptionId = attr.OptionId
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return product.Id;
        }

        public async Task<UpdateProductDto?> GetProductForEditAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.AttributeValues)
                    .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttribute)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) return null;

            var categorySpecs = await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == product.CategoryId && a.Scope == AttributeScope.Product)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            var productValuesByAttributeId = product.AttributeValues
                .Where(av => av.CategoryAttribute != null)
                .ToDictionary(av => av.CategoryAttributeId, av => av.Value ?? string.Empty);
            var legacySpecs = ParseSpecsJson(product.SpecsJson);
            var editableSpecifications = BuildEditableProductSpecifications(product.Category?.Slug, categorySpecs, productValuesByAttributeId, legacySpecs);

            return new UpdateProductDto
            {
                Name = product.Name,
                Slug = product.Slug,
                Description = product.Description,
                CategoryId = product.CategoryId,
                BrandId = product.BrandId,
                BasePrice = product.BasePrice,
                IsDraft = product.Status == ProductStatus.Draft,
                Specifications = editableSpecifications,
                ExistingImageUrls = product.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList(),
                Variants = product.Variants.Where(v => !v.IsDeleted).Select(v => new ProductVariantDto
                {
                    VariantName = v.VariantName,
                    Sku = v.Sku,
                    Price = v.Price,
                    StockQuantity = v.StockQuantity,
                    Attributes = v.AttributeValues.Select(av => new AttributeSelectionDto
                    {
                        AttributeId = av.CategoryAttributeId,
                        OptionId = av.CategoryAttributeOptionId,
                        AttributeName = av.CategoryAttribute?.Name ?? string.Empty,
                        OptionValue = av.CategoryAttributeOption?.Value ?? string.Empty
                    }).ToList()
                }).ToList()
            };
        }

        public async Task UpdateProductAsync(Guid productId, UpdateProductDto dto, Guid storeId, string userId)
        {
            var product = await _productRepository.Query()
                .Include(p => p.Images)
                .Include(p => p.AttributeValues)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.AttributeValues)
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) throw new InvalidOperationException("Product not found.");

            var slug = string.IsNullOrEmpty(dto.Slug) ? dto.Name.ToLower().Replace(" ", "-") : dto.Slug;
            await PrepareGeneratedVariantIdentityAsync(dto.Name, dto.CategoryId, dto.Variants);

            if (slug != product.Slug)
            {
                var existingSlug = await _productRepository.Query()
                    .AnyAsync(p => p.StoreId == storeId && p.Slug == slug && p.Id != productId && !p.IsDeleted);
                if (existingSlug) throw new InvalidOperationException($"Slug '{slug}' is already in use.");
            }

            product.Name = dto.Name;
            product.Slug = slug;
            product.Description = dto.Description;
            product.CategoryId = dto.CategoryId;
            product.BrandId = dto.BrandId;
            product.BasePrice = dto.BasePrice;
            product.SpecsJson = BuildSpecsJson(dto.Specifications);
            // Keep products visible on marketplace after routine edits (e.g. adding images).
            // Only move to Pending when product was not publicly visible before.
            if (dto.IsDraft)
            {
                product.Status = ProductStatus.Draft;
            }
            else if (product.Status == ProductStatus.Active || product.Status == ProductStatus.Approved)
            {
                product.Status = ProductStatus.Active;
            }
            else
            {
                product.Status = ProductStatus.Pending;
            }
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);

            foreach (var existingProductAttribute in product.AttributeValues.ToList())
            {
                await _productAttributeValueRepository.DeleteAsync(existingProductAttribute);
            }

            foreach (var spec in dto.Specifications.Where(IsValidProductSpecification))
            {
                await _productAttributeValueRepository.AddAsync(new ProductAttributeValue
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CategoryAttributeId = spec.AttributeId,
                    Value = spec.Value.Trim()
                });
            }

            if (dto.NewImages != null && dto.NewImages.Any())
            {
                var maxTotalImages = 5;
                var additionalCapacity = Math.Max(0, maxTotalImages - (product.Images.Any() ? product.Images.Count : 1));
                var imagesToUpload = dto.NewImages
                    .Take(1 + additionalCapacity)
                    .ToList();

                if (imagesToUpload.Any())
                {
                    var imageUrls = await _fileStorageService.UploadAsync(imagesToUpload);

                    var orderedImages = product.Images
                        .OrderByDescending(i => i.IsPrimary)
                        .ThenBy(i => i.SortOrder)
                        .ToList();

                    var primaryImage = orderedImages.FirstOrDefault();
                    var newPrimaryUrl = imageUrls.FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(newPrimaryUrl))
                    {
                        if (primaryImage == null)
                        {
                            primaryImage = new ProductImage
                            {
                                Id = Guid.NewGuid(),
                                ProductId = product.Id,
                                ImageUrl = newPrimaryUrl,
                                IsPrimary = true,
                                SortOrder = 0
                            };

                            await _productImageRepository.AddAsync(primaryImage);
                            product.Images.Add(primaryImage);
                        }
                        else
                        {
                            primaryImage.ImageUrl = newPrimaryUrl;
                            primaryImage.IsPrimary = true;
                            primaryImage.SortOrder = 0;
                        }
                    }

                    var secondaryImages = product.Images
                        .Where(image => primaryImage == null || image.Id != primaryImage.Id)
                        .OrderBy(image => image.SortOrder)
                        .ToList();

                    var nextSortOrder = 1;
                    foreach (var image in secondaryImages)
                    {
                        image.IsPrimary = false;
                        image.SortOrder = nextSortOrder++;
                    }

                    foreach (var imageUrl in imageUrls.Skip(1))
                    {
                        if (product.Images.Count >= maxTotalImages)
                        {
                            break;
                        }

                        var newImage = new ProductImage
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            ImageUrl = imageUrl,
                            IsPrimary = false,
                            SortOrder = nextSortOrder++
                        };

                        await _productImageRepository.AddAsync(newImage);
                        product.Images.Add(newImage);
                    }
                }
            }

            var existingVariants = product.Variants.ToList();
            var inputVariants = dto.Variants ?? new List<ProductVariantDto>();

            foreach (var ev in existingVariants.Where(v => !v.IsDeleted))
            {
                if (!inputVariants.Any(iv => iv.Sku == ev.Sku))
                {
                    ev.IsDeleted = true;
                    ev.UpdatedAt = DateTime.UtcNow;
                    await _productVariantRepository.UpdateAsync(ev);
                }
            }

            foreach (var iv in inputVariants)
            {
                var ev = existingVariants.FirstOrDefault(v => v.Sku == iv.Sku);
                if (ev != null)
                {
                    ev.VariantName = iv.VariantName;
                    ev.Price = iv.Price;
                    ev.IsActive = true;
                    ev.IsDeleted = false;
                    ev.UpdatedAt = DateTime.UtcNow;

                    if (iv.StockQuantity != ev.StockQuantity)
                    {
                        var diff = iv.StockQuantity - ev.StockQuantity;
                        await _inventoryRepository.AddAsync(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            VariantId = ev.Id,
                            Type = diff > 0 ? "AdjustmentIn" : "AdjustmentOut",
                            QuantityChange = diff,
                            Reason = "Manual adjustment during product update",
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = userId
                        });
                        ev.StockQuantity = iv.StockQuantity;
                    }

                    await _productVariantRepository.UpdateAsync(ev);

                    foreach (var attr in ev.AttributeValues.ToList())
                    {
                        await _attributeValueRepository.DeleteAsync(attr);
                    }

                    foreach (var attr in iv.Attributes)
                    {
                        await _attributeValueRepository.AddAsync(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            VariantId = ev.Id,
                            CategoryAttributeId = attr.AttributeId,
                            CategoryAttributeOptionId = attr.OptionId
                        });
                    }
                }
                else
                {
                    var newVariant = new ProductVariant
                    {
                        Id = Guid.NewGuid(),
                        ProductId = product.Id,
                        Sku = iv.Sku,
                        VariantName = iv.VariantName,
                        Price = iv.Price,
                        StockQuantity = iv.StockQuantity,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _productVariantRepository.AddAsync(newVariant);

                    if (newVariant.StockQuantity > 0)
                    {
                        await _inventoryRepository.AddAsync(new InventoryTransaction
                        {
                            Id = Guid.NewGuid(),
                            VariantId = newVariant.Id,
                            Type = "StockIn",
                            QuantityChange = newVariant.StockQuantity,
                            Reason = "Initial stock for new variant",
                            CreatedAt = DateTime.UtcNow,
                            CreatedByUserId = userId
                        });
                    }

                    foreach (var attr in iv.Attributes)
                    {
                        await _attributeValueRepository.AddAsync(new VariantAttributeValue
                        {
                            Id = Guid.NewGuid(),
                            VariantId = newVariant.Id,
                            CategoryAttributeId = attr.AttributeId,
                            CategoryAttributeOptionId = attr.OptionId
                        });
                    }
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private async Task PrepareGeneratedVariantIdentityAsync(string productName, int categoryId, List<ProductVariantDto>? variants)
        {
            if (variants == null || variants.Count == 0)
            {
                return;
            }

            var usedAttributeCombinations = new HashSet<string>(StringComparer.Ordinal);

            var optionLookup = await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == categoryId)
                .SelectMany(a => a.Options.Select(o => new
                {
                    o.Id,
                    o.Value
                }))
                .ToDictionaryAsync(x => x.Id, x => x.Value ?? string.Empty);

            var productPrefix = NormalizeSkuPart(productName, 12);
            if (string.IsNullOrWhiteSpace(productPrefix))
            {
                productPrefix = "PRODUCT";
            }

            var usedSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < variants.Count; i++)
            {
                var variant = variants[i];
                var combinationKey = string.Join(
                    "|",
                    (variant.Attributes ?? new List<AttributeSelectionDto>())
                        .Where(a => a.AttributeId > 0 && a.OptionId > 0)
                        .OrderBy(a => a.AttributeId)
                        .ThenBy(a => a.OptionId)
                        .Select(a => $"{a.AttributeId}:{a.OptionId}")
                );

                if (usedAttributeCombinations.Contains(combinationKey))
                {
                    throw new InvalidOperationException("Duplicate variant attributes are not allowed. Please choose a unique attribute combination for each variant.");
                }
                usedAttributeCombinations.Add(combinationKey);

                var selectedOptionValues = (variant.Attributes ?? new List<AttributeSelectionDto>())
                    .Select(a => optionLookup.TryGetValue(a.OptionId, out var value) ? value : string.Empty)
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Select(v => v.Trim())
                    .ToList();

                variant.VariantName = selectedOptionValues.Count > 0
                    ? string.Join(" / ", selectedOptionValues)
                    : $"Variant {i + 1}";

                var optionSkuParts = selectedOptionValues
                    .Select(v => NormalizeSkuPart(v, 8))
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .ToList();

                var skuBase = optionSkuParts.Count > 0
                    ? $"{productPrefix}-{string.Join("-", optionSkuParts)}"
                    : $"{productPrefix}-VAR{i + 1}";

                var candidateSku = skuBase;
                var suffix = 2;
                while (usedSkus.Contains(candidateSku))
                {
                    candidateSku = $"{skuBase}-{suffix++}";
                }

                usedSkus.Add(candidateSku);
                variant.Sku = candidateSku;
            }
        }

        private static string NormalizeSkuPart(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var c in value.Trim().ToUpperInvariant())
            {
                if (char.IsLetterOrDigit(c))
                {
                    builder.Append(c);
                }
                else if (char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '/')
                {
                    builder.Append('-');
                }
            }

            var normalized = builder.ToString().Trim('-');
            while (normalized.Contains("--", StringComparison.Ordinal))
            {
                normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
            }

            if (normalized.Length > maxLength)
            {
                normalized = normalized[..maxLength].Trim('-');
            }

            return normalized;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _categoryRepository.Query()
                .Where(c => !c.IsDeleted && c.IsActive)
                .ToListAsync();
        }

        public async Task<List<Brand>> GetBrandsAsync()
        {
            return await _brandRepository.Query()
                .Where(b => b.IsApproved)
                .ToListAsync();
        }

        public async Task<List<CategoryAttributeDto>> GetCategoryAttributesAsync(int categoryId)
        {
            var attributes = await _categoryAttributeRepository.Query()
                .Include(a => a.Options)
                .Where(a => a.CategoryId == categoryId
                    && a.Scope != AttributeScope.Product
                    && a.Options.Any())
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            return attributes.Select(a => new CategoryAttributeDto
            {
                Id = a.Id,
                Name = a.Name,
                FilterType = a.FilterType,
                Options = a.Options.Select(o => new CategoryAttributeOptionDto
                {
                    Id = o.Id,
                    Value = o.Value
                }).ToList()
            }).ToList();
        }

        public async Task<List<ProductSpecDto>> GetCategoryProductSpecsAsync(int categoryId)
        {
            var category = await _categoryRepository.Query()
                .Where(c => c.Id == categoryId)
                .Select(c => new { c.Slug })
                .FirstOrDefaultAsync();

            var categorySpecs = await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == categoryId && a.Scope == AttributeScope.Product)
                .OrderBy(a => a.DisplayOrder)
                .ToListAsync();

            return BuildProductSpecificationTemplates(category?.Slug, categorySpecs);
        }
        public async Task<int> CreateCategoryProductSpecificationAsync(int categoryId, string name, string? unit = null, string? valueType = null)
        {
            var normalizedName = (name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedName))
            {
                throw new InvalidOperationException("Specification name is required.");
            }

            var categoryExists = await _categoryRepository.Query().AnyAsync(c => c.Id == categoryId && !c.IsDeleted);
            if (!categoryExists)
            {
                throw new InvalidOperationException("Category not found.");
            }

            var existing = await _categoryAttributeRepository.Query()
                .AnyAsync(a => a.CategoryId == categoryId
                    && a.Scope == AttributeScope.Product
                    && a.Name.ToLower() == normalizedName.ToLower());

            if (existing)
            {
                throw new InvalidOperationException("This specification already exists in the selected category.");
            }

            var nextDisplayOrder = (await _categoryAttributeRepository.Query()
                .Where(a => a.CategoryId == categoryId && a.Scope == AttributeScope.Product)
                .MaxAsync(a => (int?)a.DisplayOrder) ?? 0) + 1;

            var attribute = new CategoryAttribute
            {
                CategoryId = categoryId,
                Name = normalizedName,
                Scope = AttributeScope.Product,
                FilterType = "Checkbox",
                IsFilterable = false,
                IsComparable = true,
                DisplayOrder = nextDisplayOrder,
                Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
                ValueType = string.IsNullOrWhiteSpace(valueType) ? null : valueType.Trim()
            };

            await _categoryAttributeRepository.AddAsync(attribute);
            await _unitOfWork.SaveChangesAsync();

            return attribute.Id;
        }

        public async Task ToggleProductStatusAsync(Guid productId, Guid storeId)
        {
            var product = await _productRepository.Query()
                .FirstOrDefaultAsync(p => p.Id == productId && p.StoreId == storeId && !p.IsDeleted);

            if (product == null) throw new InvalidOperationException("Product not found.");

            if (product.Status == ProductStatus.Active || product.Status == ProductStatus.Approved)
            {
                product.Status = ProductStatus.Inactive;
            }
            else if (product.Status == ProductStatus.Inactive)
            {
                product.Status = ProductStatus.Active;
            }
            else
            {
                throw new InvalidOperationException("Only active or approved products can be deactivated.");
            }
            product.UpdatedAt = DateTime.UtcNow;

            await _productRepository.UpdateAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<int> CreateBrandByNameAsync(string name)
        {
            var slug = name.ToLower().Replace(" ", "-");
            var brand = new Brand
            {
                Name = name,
                Slug = slug,
                IsApproved = true,
                CreatedAt = DateTime.UtcNow
            };

            await _brandRepository.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            return brand.Id;
        }

        public async Task<int> CreateCategoryByNameAsync(string name)
        {
            var slug = name.ToLower().Replace(" ", "-");
            var category = new Category
            {
                Name = name,
                Slug = slug,
                IsActive = true
            };

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return category.Id;
        }

        private static Dictionary<string, string> BuildProductSpecificationDictionary(
            string? categorySlug,
            IEnumerable<ProductAttributeValue> structuredAttributes,
            IReadOnlyDictionary<string, string> legacySpecs)
        {
            var specs = structuredAttributes
                .Where(av => av.CategoryAttribute != null)
                .OrderBy(av => av.CategoryAttribute.DisplayOrder)
                .ThenBy(av => av.CategoryAttribute.Name)
                .ToDictionary(av => av.CategoryAttribute.Name, FormatProductAttributeValue, StringComparer.OrdinalIgnoreCase);

            foreach (var definition in SeededProductSpecificationCatalog.GetTemplate(categorySlug))
            {
                if (specs.ContainsKey(definition.DisplayName))
                {
                    continue;
                }

                var legacyValue = SeededProductSpecificationCatalog.FindLegacyValue(categorySlug, legacySpecs, definition.DisplayName);
                if (!string.IsNullOrWhiteSpace(legacyValue))
                {
                    specs[definition.DisplayName] = legacyValue;
                }
            }

            foreach (var legacySpec in legacySpecs)
            {
                if (specs.ContainsKey(legacySpec.Key)
                    || SeededProductSpecificationCatalog.GetDefinition(categorySlug, legacySpec.Key) != null)
                {
                    continue;
                }

                specs[legacySpec.Key] = legacySpec.Value;
            }

            return specs;
        }

        private static List<ProductSpecDto> BuildEditableProductSpecifications(
            string? categorySlug,
            IReadOnlyCollection<CategoryAttribute> categorySpecs,
            IReadOnlyDictionary<int, string> productValuesByAttributeId,
            IReadOnlyDictionary<string, string> legacySpecs)
        {
            var specs = BuildProductSpecificationTemplates(categorySlug, categorySpecs);

            foreach (var spec in specs)
            {
                string value = string.Empty;
                if (spec.AttributeId > 0 && productValuesByAttributeId.TryGetValue(spec.AttributeId, out var structuredValue))
                {
                    value = structuredValue;
                }
                else
                {
                    value = SeededProductSpecificationCatalog.FindLegacyValue(categorySlug, legacySpecs, spec.Key) ?? string.Empty;
                }

                spec.Value = NormalizeSpecValue(value, spec.Unit);
            }

            foreach (var legacySpec in legacySpecs)
            {
                if (specs.Any(spec => spec.Key.Equals(legacySpec.Key, StringComparison.OrdinalIgnoreCase))
                    || SeededProductSpecificationCatalog.GetDefinition(categorySlug, legacySpec.Key) != null)
                {
                    continue;
                }

                specs.Add(new ProductSpecDto
                {
                    AttributeId = 0,
                    Key = legacySpec.Key,
                    Value = legacySpec.Value
                });
            }

            return specs;
        }

        private static List<ProductSpecDto> BuildProductSpecificationTemplates(string? categorySlug, IReadOnlyCollection<CategoryAttribute> categorySpecs)
        {
            var specs = new List<ProductSpecDto>();
            var categorySpecsByName = categorySpecs
                .ToDictionary(attr => attr.Name, attr => attr, StringComparer.OrdinalIgnoreCase);

            foreach (var definition in SeededProductSpecificationCatalog.GetTemplate(categorySlug))
            {
                if (categorySpecsByName.TryGetValue(definition.DisplayName, out var attr))
                {
                    specs.Add(new ProductSpecDto
                    {
                        AttributeId = attr.Id,
                        Key = attr.Name,
                        Unit = attr.Unit,
                        ValueType = attr.ValueType,
                        Value = string.Empty
                    });
                }
                else
                {
                    specs.Add(new ProductSpecDto
                    {
                        AttributeId = 0,
                        Key = definition.DisplayName,
                        Unit = definition.Unit,
                        ValueType = definition.ValueType,
                        Value = string.Empty
                    });
                }
            }

            foreach (var attr in categorySpecs.OrderBy(a => a.DisplayOrder).ThenBy(a => a.Name))
            {
                if (specs.Any(spec => spec.Key.Equals(attr.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                specs.Add(new ProductSpecDto
                {
                    AttributeId = attr.Id,
                    Key = attr.Name,
                    Unit = attr.Unit,
                    ValueType = attr.ValueType,
                    Value = string.Empty
                });
            }

            return specs;
        }

        private static bool IsValidProductSpecification(ProductSpecDto spec)
        {
            return spec.AttributeId > 0 && !string.IsNullOrWhiteSpace(spec.Value);
        }

        private static Dictionary<string, string> ParseSpecsJson(string rawJson)
        {
            if (string.IsNullOrWhiteSpace(rawJson) || rawJson == "{}")
            {
                return new Dictionary<string, string>();
            }

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(rawJson) ?? new Dictionary<string, string>();
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        private static string BuildSpecsJson(IEnumerable<ProductSpecDto>? specs)
        {
            if (specs == null)
            {
                return "{}";
            }

            var payload = specs
                .Where(s => !string.IsNullOrWhiteSpace(s.Key) && !string.IsNullOrWhiteSpace(s.Value))
                .GroupBy(s => s.Key)
                .ToDictionary(g => g.Key, g => g.Last().Value.Trim());

            return payload.Count == 0 ? "{}" : JsonSerializer.Serialize(payload);
        }

        private static string NormalizeSpecValue(string value, string? unit)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (string.IsNullOrWhiteSpace(unit))
            {
                return value.Trim();
            }

            var normalized = value.Trim();
            if (normalized.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized.Substring(0, normalized.Length - unit.Length).Trim();
            }

            return normalized;
        }

        private static string FormatProductAttributeValue(ProductAttributeValue attributeValue)
        {
            var rawValue = (attributeValue.CategoryAttributeOption?.Value ?? attributeValue.Value ?? string.Empty).Trim();
            var unit = attributeValue.CategoryAttribute?.Unit;

            if (string.IsNullOrWhiteSpace(unit) || rawValue.EndsWith(unit, StringComparison.OrdinalIgnoreCase))
            {
                return rawValue;
            }

            return $"{rawValue} {unit}";
        }
    }
}







