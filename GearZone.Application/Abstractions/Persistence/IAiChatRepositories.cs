using GearZone.Application.Common.Models;
using GearZone.Application.Features.AiChat.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence;

public interface IAiConversationRepository : IRepository<AiConversation, Guid>
{
    Task<AiConversation?> GetOwnedAsync(Guid id, AiChatActor actor, CancellationToken ct = default);
    Task<PagedResult<AiConversation>> GetForCustomerAsync(string customerUserId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<int> DeleteExpiredAsync(DateTime utcNow, CancellationToken ct = default);
}

public interface IAiMessageRepository : IRepository<AiMessage, Guid>
{
    Task<List<AiMessage>> GetPageAsync(Guid conversationId, DateTime? beforeUtc, int pageSize, CancellationToken ct = default);
    Task<List<AiMessage>> GetRecentCompletedAsync(Guid conversationId, int take, CancellationToken ct = default);
    Task<AiMessage?> GetByClientIdAsync(Guid conversationId, Guid clientMessageId, string role, CancellationToken ct = default);
}

public interface IAiKnowledgeRepository : IRepository<AiKnowledgeArticle, Guid>
{
    Task<PagedResult<AiKnowledgeArticle>> SearchAsync(AiKnowledgeQueryDto query, CancellationToken ct = default);
    Task<List<AiKnowledgeArticle>> SearchPublishedAsync(string query, string? category, int take, CancellationToken ct = default);
    Task<AiKnowledgeArticle?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<Dictionary<string, int>> GetStatusCountsAsync(CancellationToken ct = default);
}
