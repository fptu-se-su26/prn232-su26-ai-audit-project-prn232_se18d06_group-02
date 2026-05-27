using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AutoMapper;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Features.Admin.Mappings;

public class AdminProductProfile : Profile
{
    public AdminProductProfile()
    {
        // Index Listing Map
        CreateMap<Product, AdminProductDto>()
            .ForMember(dest => dest.Sku, opt => opt.MapFrom(src => 
                src.Variants != null && src.Variants.Any() ? src.Variants.FirstOrDefault().Sku : string.Empty))
            .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => 
                src.Store != null ? src.Store.StoreName : string.Empty))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => 
                src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.BasePrice))
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => 
                src.Variants != null ? src.Variants.Sum(v => v.StockQuantity) : 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.MapFrom(src => 
                src.Images != null && src.Images.Any()
                    ? (src.Images.FirstOrDefault(i => i.IsPrimary) != null 
                        ? src.Images.FirstOrDefault(i => i.IsPrimary).ImageUrl 
                        : src.Images.FirstOrDefault().ImageUrl)
                    : null));

        // Details Maps
        CreateMap<Category, AdminCategoryInfoDto>();
        CreateMap<Brand, AdminBrandInfoDto>();
        CreateMap<Store, AdminStoreInfoDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.StoreName))
            .ForMember(dest => dest.JoinedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.AvatarUrl, opt => opt.MapFrom(src => src.LogoUrl))
            .ForMember(dest => dest.VendorId, opt => opt.MapFrom(src => "ST-" + src.Id.ToString().ToUpper().Substring(0, 8)));

        CreateMap<VariantAttributeValue, AdminVariantAttributeDto>()
            .ForMember(dest => dest.AttributeName, opt => opt.MapFrom(src => 
                src.CategoryAttribute != null ? src.CategoryAttribute.Name : string.Empty))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src =>
                src.CategoryAttributeOption != null ? src.CategoryAttributeOption.Value : string.Empty));

        CreateMap<ProductVariant, AdminProductVariantDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.VariantName))
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.StockQuantity))
            .ForMember(dest => dest.Attributes, opt => opt.MapFrom(src => src.AttributeValues));

        CreateMap<Product, AdminProductDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.CommissionRate, opt => opt.MapFrom(src => 
                src.Store != null ? src.Store.CommissionRate * 100 : 0)) // Showing as percentage
            .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => 
                src.Variants != null ? src.Variants.Sum(v => v.StockQuantity) : 0))
            .ForMember(dest => dest.Images, opt => opt.MapFrom(src => 
                src.Images != null ? src.Images.OrderBy(i => i.SortOrder).Select(i => i.ImageUrl).ToList() : new List<string>()))
            .ForMember(dest => dest.Store, opt => opt.MapFrom(src => src.Store != null ? new AdminStoreInfoDto {
                Id = src.Store.Id,
                Name = src.Store.StoreName,
                Slug = src.Store.Slug,
                JoinedAt = src.Store.CreatedAt,
                AvatarUrl = src.Store.LogoUrl,
                VendorId = "ST-" + src.Store.Id.ToString().ToUpper().Substring(0, 8)
            } : null))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => new AdminCategoryInfoDto {
                Id = src.Category.Id,
                Name = src.Category.Name,
                Slug = src.Category.Slug
            }))
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => new AdminBrandInfoDto {
                Id = src.Brand.Id,
                Name = src.Brand.Name,
                Slug = src.Brand.Slug
            }))
            .ForMember(dest => dest.Specs, opt => opt.MapFrom(src =>
                MapSpecs(src)));
    }

    private static List<AdminProductSpecDto> MapSpecs(Product src)
    {
        var specs = new List<(string Name, string Value, int? Order)>();

        // 1. Add Product-level Attributes (Standard structured)
        if (src.AttributeValues != null)
        {
            foreach (var av in src.AttributeValues)
            {
                if (av.CategoryAttribute == null) continue;

                var rawValue = (av.CategoryAttributeOption?.Value ?? av.Value ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(rawValue)) continue;

                var unit = av.CategoryAttribute.Unit;
                var formattedValue = (string.IsNullOrWhiteSpace(unit) || rawValue.EndsWith(unit, System.StringComparison.OrdinalIgnoreCase))
                    ? rawValue
                    : $"{rawValue} {unit}";

                specs.Add((av.CategoryAttribute.Name, formattedValue, av.CategoryAttribute.DisplayOrder));
            }
        }

        // 2. Add Variant-level Attributes
        if (src.Variants != null)
        {
            foreach (var variant in src.Variants.Where(v => !v.IsDeleted))
            {
                if (variant.AttributeValues == null) continue;

                foreach (var av in variant.AttributeValues)
                {
                    if (av.CategoryAttribute == null) continue;

                    var rawValue = (av.CategoryAttributeOption?.Value ?? string.Empty).Trim();
                    if (string.IsNullOrEmpty(rawValue)) continue;

                    var unit = av.CategoryAttribute.Unit;
                    var formattedValue = (string.IsNullOrWhiteSpace(unit) || rawValue.EndsWith(unit, System.StringComparison.OrdinalIgnoreCase))
                        ? rawValue
                        : $"{rawValue} {unit}";

                    specs.Add((av.CategoryAttribute.Name, formattedValue, av.CategoryAttribute.DisplayOrder));
                }
            }
        }

        // 3. Add Custom Specs from SpecsJson (Fallback for legacy or loose data)
        if (!string.IsNullOrWhiteSpace(src.SpecsJson) && src.SpecsJson != "{}")
        {
            try
            {
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, string>>(src.SpecsJson);
                if (jsonDict != null)
                {
                    foreach (var kvp in jsonDict)
                    {
                        if (!string.IsNullOrWhiteSpace(kvp.Value) && !specs.Any(s => s.Name == kvp.Key))
                        {
                            specs.Add((kvp.Key, kvp.Value.Trim(), 9999));
                        }
                    }
                }
            }
            catch { /* Ignore invalid JSON */ }
        }

        return specs
            .GroupBy(x => x.Name)
            .Select(g => new AdminProductSpecDto
            {
                AttributeName = g.Key,
                // We keep multiple values if they are different (e.g. from different variants)
                Values = g.Select(x => x.Value).Distinct().OrderBy(v => v).ToList()
            })
            // Sort by Order (if available) or by Name
            .OrderBy(x => specs.FirstOrDefault(s => s.Name == x.AttributeName).Order ?? 9999)
            .ThenBy(x => x.AttributeName)
            .ToList();
    }
}
