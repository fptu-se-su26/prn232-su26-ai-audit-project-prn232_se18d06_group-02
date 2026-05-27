using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class CategoryQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; }
        public int? ParentId { get; set; }
    }
}
