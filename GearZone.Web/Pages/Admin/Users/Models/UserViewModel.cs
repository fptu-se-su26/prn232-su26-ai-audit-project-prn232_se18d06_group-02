using System;
using System.Collections.Generic;

namespace GearZone.Web.Pages.Admin.Users.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Role { get; set; }
        public bool IsDeleted { get; set; }
    }
}
