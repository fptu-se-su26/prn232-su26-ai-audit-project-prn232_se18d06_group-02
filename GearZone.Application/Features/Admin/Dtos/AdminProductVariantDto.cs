using System.Collections.Generic;

namespace GearZone.Application.Features.Admin.Dtos
{
    public class AdminProductVariantDto
    {
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }

        public List<AdminVariantAttributeDto> Attributes { get; set; } = new List<AdminVariantAttributeDto>();
    }

    public class AdminVariantAttributeDto
    {
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
