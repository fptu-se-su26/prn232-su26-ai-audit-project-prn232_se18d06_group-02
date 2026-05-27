using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IChatMessageRepository : IRepository<ChatMessage, Guid>
    {
        Task<List<ChatMessageItemDto>> GetRecentMessagesAsync(Guid conversationId, int take, CancellationToken ct = default);
        Task<int> GetMessageCountAsync(Guid conversationId, CancellationToken ct = default);
        Task<int> MarkAsReadAsync(Guid conversationId, string readerUserId, DateTime readAt, CancellationToken ct = default);
    }
}
