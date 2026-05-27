namespace GearZone.Domain.Entities
{
    public class Conversation : Entity<Guid>
    {
        public string BuyerUserId { get; set; } = string.Empty;
        public Guid StoreId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastMessageAt { get; set; } = DateTime.UtcNow;

        public ApplicationUser BuyerUser { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
