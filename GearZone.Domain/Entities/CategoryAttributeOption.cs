using System;
using System.Collections.Generic;

namespace GearZone.Domain.Entities
{
    public class CategoryAttributeOption : Entity<int>
    {
        public int CategoryAttributeId { get; set; }
        public string Value { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }

        public CategoryAttribute CategoryAttribute { get; set; } = null!;
        public ICollection<VariantAttributeValue> VariantSelections { get; set; } = new List<VariantAttributeValue>();
    }
}
