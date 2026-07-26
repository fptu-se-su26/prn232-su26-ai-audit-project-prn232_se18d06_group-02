using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities;

public class AiConversation : Entity<Guid>
{
    public string? CustomerUserId { get; set; }
    public string? GuestTokenHash { get; set; }
    public string Title { get; set; } = "New conversation";
    public AiConversationStatus Status { get; set; } = AiConversationStatus.Active;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastActivityAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public ApplicationUser? CustomerUser { get; set; }
    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}
