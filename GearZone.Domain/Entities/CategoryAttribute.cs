using System.Collections.Generic;
using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities
{
    public class CategoryAttribute : Entity<int>
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FilterType { get; set; } = "Checkbox";
        public int DisplayOrder { get; set; }
        public bool IsFilterable { get; set; } = true;
        public AttributeScope Scope { get; set; } = AttributeScope.Variant;
        public bool IsComparable { get; set; } = false;
        public string? ValueType { get; set; } // e.g., "Number", "String", "Frequency"
        public string? Unit { get; set; }      // e.g., "MHz", "GB", "Cores"

        public Category Category { get; set; } = null!;
        public ICollection<CategoryAttributeOption> Options { get; set; } = new List<CategoryAttributeOption>();
    }
}
