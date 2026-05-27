namespace GearZone.Application.Features.Admin.Dtos
{
    public class CategoryAttributeDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = "Checkbox";
        public int DisplayOrder { get; set; }
        public bool IsFilterable { get; set; } = true;
        public List<CategoryAttributeOptionDto> Options { get; set; } = new();
    }

    public class CategoryAttributeOptionDto
    {
        public int Id { get; set; }
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class SaveCategoryAttributesRequest
    {
        public int CategoryId { get; set; }
        public List<CategoryAttributeDto> Attributes { get; set; } = new();
    }
}
