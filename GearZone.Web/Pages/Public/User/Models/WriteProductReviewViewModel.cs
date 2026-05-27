using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Pages.Public.User.Models
{
    public class WriteProductReviewViewModel
    {
        [Required]
        public Guid OrderItemId { get; set; }

        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }

        [StringLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters.")]
        public string? Comment { get; set; }
    }
}
