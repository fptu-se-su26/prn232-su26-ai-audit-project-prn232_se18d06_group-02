using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Pages.Public.User.Models
{
    public class StoreRegistrationViewModel
    {
        [Required(ErrorMessage = "Store Name is required")]
        public string StoreName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tax Code is required")]
        public string TaxCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string AddressLine { get; set; } = string.Empty;

        [Required(ErrorMessage = "Province is required")]
        public string Province { get; set; } = string.Empty;
    }
}
