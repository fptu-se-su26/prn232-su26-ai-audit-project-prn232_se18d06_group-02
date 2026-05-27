using GearZone.Application.Common.Models;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class UserQueryDto : PaginationRequest
    {
        public string? SearchTerm { get; set; }
        public string? Role { get; set; }
        public bool? IsActive { get; set; }
    }
}
