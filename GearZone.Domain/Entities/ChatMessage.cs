namespace GearZone.Domain.Entities
{
    public class ChatMessage : Entity<Guid>
    {
        public Guid ConversationId { get; set; }
        public string SenderUserId { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }

        public Conversation Conversation { get; set; } = null!;
        public ApplicationUser SenderUser { get; set; } = null!;
    }
}
