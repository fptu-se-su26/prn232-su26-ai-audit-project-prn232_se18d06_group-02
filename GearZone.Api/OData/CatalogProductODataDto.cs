using System.ComponentModel.DataAnnotations;

namespace GearZone.Api.OData;

/// <summary>
/// Public, read-only product projection exposed through OData.
/// </summary>
public sealed class CatalogProductODataDto
{
    [Key]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int SoldCount { get; set; }
    public bool InStock { get; set; }
    public DateTime CreatedAt { get; set; }
}
