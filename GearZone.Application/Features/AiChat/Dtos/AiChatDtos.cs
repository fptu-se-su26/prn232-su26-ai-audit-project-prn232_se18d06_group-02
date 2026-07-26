using System.ComponentModel.DataAnnotations;
using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.AiChat.Dtos;

public sealed record AiChatActor(string? CustomerUserId, string? GuestTokenHash)
{
    public bool IsCustomer => !string.IsNullOrWhiteSpace(CustomerUserId);
}

public sealed class CreateAiConversationDto
{
    public string? ProductSlug { get; set; }
    public string? StoreSlug { get; set; }
}

public sealed class SendAiMessageDto
{
    [Required]
    public Guid ClientMessageId { get; set; }

    [Required, StringLength(2000, MinimumLength = 1)]
    public string Message { get; set; } = string.Empty;

    public AiChatPageContextDto? PageContext { get; set; }
}

public sealed class AiChatPageContextDto
{
    [StringLength(200)]
    public string? ProductSlug { get; set; }

    [StringLength(200)]
    public string? StoreSlug { get; set; }
}

public sealed class AiConversationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastActivityAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
}

public sealed class AiMessageDto
{
    public Guid Id { get; set; }
    public Guid ClientMessageId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AiChatMessageMetadataDto Metadata { get; set; } = new();
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class AiConversationMessagesDto
{
    public AiConversationDto Conversation { get; set; } = new();
    public List<AiMessageDto> Messages { get; set; } = new();
    public DateTime? NextCursor { get; set; }
}

public sealed class AiChatMessageMetadataDto
{
    public List<AiProductCardDto> Products { get; set; } = new();
    public List<AiOrderCardDto> Orders { get; set; } = new();
    public List<AiChatSourceDto> Sources { get; set; } = new();
    public List<AiSuggestedActionDto> Actions { get; set; } = new();
}

public sealed class AiProductCardDto
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string BrandName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string StoreSlug { get; set; } = string.Empty;
    public decimal Rating { get; set; }
    public bool IsInStock { get; set; }
    public string Url { get; set; } = string.Empty;
}

public sealed class AiOrderCardDto
{
    public Guid SubOrderId { get; set; }
    public long OrderCode { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public string Url { get; set; } = string.Empty;
}

public sealed class AiChatSourceDto
{
    public string Type { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}

public sealed class AiSuggestedActionDto
{
    public string Type { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? StoreSlug { get; set; }
    public string? ProductSlug { get; set; }
}

public sealed class AiChatProgress
{
    public string Type { get; init; } = string.Empty;
    public object Data { get; init; } = new { };
}

public sealed class AiChatSendResult
{
    public AiMessageDto UserMessage { get; set; } = new();
    public AiMessageDto AssistantMessage { get; set; } = new();
    public bool WasDuplicate { get; set; }
}

public sealed class AiKnowledgeQueryDto
{
    public string? Search { get; set; }
    public AiKnowledgeCategory? Category { get; set; }
    public AiKnowledgeStatus? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class AiKnowledgeArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public AiKnowledgeCategory Category { get; set; }
    public string Keywords { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public AiKnowledgeStatus Status { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
}

public class SaveAiKnowledgeArticleDto
{
    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(200)]
    [RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$", ErrorMessage = "Slug must contain lowercase letters, numbers, and single hyphens only.")]
    public string Slug { get; set; } = string.Empty;

    public AiKnowledgeCategory Category { get; set; } = AiKnowledgeCategory.General;

    [StringLength(1000)]
    public string Keywords { get; set; } = string.Empty;

    [Required, StringLength(20000, MinimumLength = 10)]
    public string Content { get; set; } = string.Empty;
}

public sealed class AiKnowledgeListDto
{
    public PagedResult<AiKnowledgeArticleDto> Articles { get; set; } = new();
    public Dictionary<string, int> StatusCounts { get; set; } = new();
}
