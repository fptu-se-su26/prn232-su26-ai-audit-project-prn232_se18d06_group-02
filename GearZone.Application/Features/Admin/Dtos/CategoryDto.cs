namespace GearZone.Application.Features.Admin.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int ProductCount { get; set; }
        public decimal Revenue { get; set; }
        public string Level => ParentId == null ? "Root" : "Sub 1";
        public List<CategoryDto> Children { get; set; } = new();
    }
}
