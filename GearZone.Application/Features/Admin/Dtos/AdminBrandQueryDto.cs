using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos;

public class AdminBrandQueryDto : PaginationRequest
{
    public string? SearchTerm { get; set; }
    public bool? IsApproved { get; set; }
}
