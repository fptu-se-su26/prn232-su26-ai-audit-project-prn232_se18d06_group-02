using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Pages.Admin.Users.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at most {1} characters long.")]
        public string FullName { get; set; } = string.Empty;

        public string? Email { get; set; }

        [Display(Name = "Phone Number")]
        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [Display(Name = "User Role")]
        public string Role { get; set; } = string.Empty;
    }
}
