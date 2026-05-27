namespace GearZone.Application.Features.Cart.DTOs
{
    public class StoreCartGroupDto
    {
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public ICollection<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public decimal StoreSubtotal { get; set; }
    }
}
