using Microsoft.AspNetCore.Http;

namespace GearZone.Application.Features.Admin.Dtos;

public class CreateBrandDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public IFormFile? LogoFile { get; set; }
    public bool IsApproved { get; set; } = true;
}

public class EditBrandDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public IFormFile? LogoFile { get; set; }
    public bool IsApproved { get; set; } = true;
}
