using GearZone.Application.Common.Models;
using GearZone.Application.Features.AiChat.Dtos;

namespace GearZone.Application.Abstractions.Services;

public interface IAiChatService
{
    bool IsEnabled { get; }
    Task<AiConversationDto> CreateConversationAsync(AiChatActor actor, CancellationToken ct = default);
    Task<PagedResult<AiConversationDto>> GetConversationsAsync(string customerUserId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<AiConversationMessagesDto?> GetMessagesAsync(Guid conversationId, AiChatActor actor, DateTime? beforeUtc, int pageSize, CancellationToken ct = default);
    Task<AiChatSendResult> SendMessageAsync(
        Guid conversationId,
        AiChatActor actor,
        SendAiMessageDto request,
        Func<AiChatProgress, CancellationToken, Task>? progress = null,
        CancellationToken ct = default);
    Task<bool> DeleteConversationAsync(Guid conversationId, AiChatActor actor, CancellationToken ct = default);
}

public interface IAiChatToolExecutor
{
    Task<GearZone.Application.Abstractions.External.AiToolExecutionResult> ExecuteAsync(
        string toolName,
        string argumentsJson,
        AiChatActor actor,
        CancellationToken ct = default);
}

public interface IAiKnowledgeService
{
    Task<AiKnowledgeListDto> SearchAsync(AiKnowledgeQueryDto query, CancellationToken ct = default);
    Task<AiKnowledgeArticleDto?> GetAsync(Guid id, CancellationToken ct = default);
    Task<AiKnowledgeArticleDto> CreateAsync(SaveAiKnowledgeArticleDto input, string userId, CancellationToken ct = default);
    Task<AiKnowledgeArticleDto?> UpdateAsync(Guid id, SaveAiKnowledgeArticleDto input, string userId, CancellationToken ct = default);
    Task<bool> PublishAsync(Guid id, string userId, CancellationToken ct = default);
    Task<bool> ArchiveAsync(Guid id, string userId, CancellationToken ct = default);
}
