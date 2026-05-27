using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Pages.Public.Auth.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email or Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}
