namespace GearZone.Application.Features.Cart.DTOs
{
    public class CartDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ICollection<StoreCartGroupDto> StoreGroups { get; set; } = new List<StoreCartGroupDto>();
        public decimal TotalPrice { get; set; }
    }
}
