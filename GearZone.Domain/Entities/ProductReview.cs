namespace GearZone.Domain.Entities
{
    public class ProductReview : Entity<Guid>
    {
        public Guid OrderItemId { get; set; }
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;

        public int Rating { get; set; }
        public string? Comment { get; set; }

        public string? SellerReplyContent { get; set; }
        public DateTime? SellerReplyAt { get; set; }
        public DateTime? SellerReplyUpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public OrderItem OrderItem { get; set; } = null!;
        public Product Product { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public ApplicationUser BuyerUser { get; set; } = null!;
    }
}
