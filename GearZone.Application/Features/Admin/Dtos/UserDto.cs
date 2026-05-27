using System;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; }
        public bool IsDeleted { get; set; }
    }
}
