using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities;

public class AiMessage : Entity<Guid>
{
    public Guid ConversationId { get; set; }
    public Guid ClientMessageId { get; set; }
    public AiMessageRole Role { get; set; }
    public AiMessageStatus Status { get; set; } = AiMessageStatus.Pending;
    public string Content { get; set; } = string.Empty;
    public string MetadataJson { get; set; } = "{}";
    public string? Model { get; set; }
    public int? InputTokens { get; set; }
    public int? OutputTokens { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public AiConversation Conversation { get; set; } = null!;
}
