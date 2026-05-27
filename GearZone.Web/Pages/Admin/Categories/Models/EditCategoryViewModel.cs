using System.ComponentModel.DataAnnotations;

namespace GearZone.Web.Pages.Admin.Categories.Models
{
    public class EditCategoryViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug is required")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters")]
        [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Slug can only contain lowercase letters, numbers, and hyphens")]
        public string Slug { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        public bool IsActive { get; set; }
    }
}
