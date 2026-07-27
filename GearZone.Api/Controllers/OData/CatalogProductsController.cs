using GearZone.Api.OData;
using GearZone.Domain.Enums;
using GearZone.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Api.Controllers.OData;

/// <summary>
/// Read-only OData endpoint for querying the public product catalog.
/// </summary>
[AllowAnonymous]
public sealed class CatalogProductsController : ODataController
{
    private readonly ApplicationDbContext _dbContext;

    public CatalogProductsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // GET /odata/CatalogProducts?$filter=Price le 1000000&$orderby=SoldCount desc
    [EnableQuery(PageSize = 20, MaxTop = 100, EnsureStableOrdering = true)]
    public IQueryable<CatalogProductODataDto> Get()
    {
        return _dbContext.Products
            .AsNoTracking()
            .Where(product =>
                !product.IsDeleted &&
                product.Status == ProductStatus.Active)
            .Select(product => new CatalogProductODataDto
            {
                Id = product.Id,
                Name = product.Name,
                Slug = product.Slug,
                BrandName = product.Brand.Name,
                CategoryName = product.Category.Name,
                StoreName = product.Store.StoreName,
                Price = product.BasePrice,
                SoldCount = product.SoldCount,
                InStock = product.Variants.Any(variant =>
                    variant.IsActive &&
                    !variant.IsDeleted &&
                    variant.StockQuantity > 0),
                CreatedAt = product.CreatedAt
            });
    }
}
