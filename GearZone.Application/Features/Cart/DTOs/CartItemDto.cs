namespace GearZone.Application.Features.Cart.DTOs
{
    public class CartItemDto
    {
        public Guid Id { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? VariantName { get; set; } // e.g., "Color: Red, Size: L"
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal Subtotal => Price * Quantity;
    }
}
