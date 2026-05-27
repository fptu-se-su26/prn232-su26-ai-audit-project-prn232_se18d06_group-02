using System.ComponentModel.DataAnnotations;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class EditUserDto
    {
        public string Id { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Phone]
        public string? PhoneNumber { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
