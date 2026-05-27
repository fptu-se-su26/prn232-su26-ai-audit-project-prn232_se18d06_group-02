using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;

namespace GearZone.Application.Abstractions.Persistence
{
    public interface IConversationRepository : IRepository<Conversation, Guid>
    {
        Task<Conversation?> GetByBuyerAndStoreAsync(string buyerUserId, Guid storeId, CancellationToken ct = default);
        Task<Conversation?> GetByIdWithParticipantsAsync(Guid conversationId, CancellationToken ct = default);
        Task<PagedResult<ChatConversationListItemDto>> GetBuyerInboxAsync(string buyerUserId, ChatInboxQueryDto query, CancellationToken ct = default);
        Task<PagedResult<ChatConversationListItemDto>> GetSellerInboxAsync(string ownerUserId, ChatInboxQueryDto query, CancellationToken ct = default);
        Task<List<ChatCounterpartScopeOptionDto>> GetBuyerCounterpartScopeOptionsAsync(string buyerUserId, CancellationToken ct = default);
        Task<List<ChatCounterpartScopeOptionDto>> GetSellerCounterpartScopeOptionsAsync(string ownerUserId, CancellationToken ct = default);
        Task<ChatConversationListItemDto?> GetBuyerConversationListItemAsync(string buyerUserId, Guid conversationId, CancellationToken ct = default);
        Task<ChatConversationListItemDto?> GetSellerConversationListItemAsync(string ownerUserId, Guid conversationId, CancellationToken ct = default);
        Task<int> GetBuyerUnreadCountAsync(string buyerUserId, CancellationToken ct = default);
        Task<int> GetSellerUnreadCountAsync(string ownerUserId, CancellationToken ct = default);
    }
}
