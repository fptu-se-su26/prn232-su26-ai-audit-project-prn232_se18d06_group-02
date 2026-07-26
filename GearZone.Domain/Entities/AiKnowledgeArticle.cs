using GearZone.Domain.Enums;

namespace GearZone.Domain.Entities;

public class AiKnowledgeArticle : Entity<Guid>
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public AiKnowledgeCategory Category { get; set; } = AiKnowledgeCategory.General;
    public string Keywords { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AiKnowledgeStatus Status { get; set; } = AiKnowledgeStatus.Draft;
    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}
