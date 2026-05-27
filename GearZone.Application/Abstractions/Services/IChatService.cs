using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;

namespace GearZone.Application.Abstractions.Services
{
    public interface IChatService
    {
        Task<PagedResult<ChatConversationListItemDto>> GetBuyerInboxAsync(string userId, ChatInboxQueryDto query);
        Task<PagedResult<ChatConversationListItemDto>> GetSellerInboxAsync(string ownerUserId, ChatInboxQueryDto query);
        Task<List<ChatCounterpartScopeOptionDto>> GetBuyerCounterpartScopeOptionsAsync(string userId);
        Task<List<ChatCounterpartScopeOptionDto>> GetSellerCounterpartScopeOptionsAsync(string ownerUserId);
        Task<ChatThreadDto?> GetBuyerThreadAsync(string userId, Guid conversationId, ChatThreadQueryDto query);
        Task<ChatThreadDto?> GetSellerThreadAsync(string ownerUserId, Guid conversationId, ChatThreadQueryDto query);
        Task<ChatWidgetBootstrapDto> GetBuyerWidgetBootstrapAsync(string userId, ChatWidgetBootstrapQueryDto query);
        Task<Guid?> EnsureBuyerConversationAsync(string userId, string storeSlug);
        Task<Guid?> EnsureSellerConversationFromSubOrderAsync(string ownerUserId, Guid subOrderId);
        Task<ChatSendMessageResultDto> SendMessageAsync(string userId, SendChatMessageDto dto);
        Task<int> MarkConversationReadAsync(string userId, Guid conversationId);
        Task<int> GetBuyerUnreadCountAsync(string userId);
        Task<int> GetSellerUnreadCountAsync(string ownerUserId);
        Task<PagedResult<SellerChatOrderListItemDto>> GetSellerChatOrdersAsync(string ownerUserId, SellerChatOrderQueryDto query);
        Task<SellerChatOrderDetailDto?> GetSellerChatOrderDetailAsync(string ownerUserId, Guid subOrderId);
        Task<bool> ApproveSellerOrderAsync(string ownerUserId, Guid subOrderId);
        Task<bool> RejectSellerOrderAsync(string ownerUserId, Guid subOrderId);
        Task<bool> MarkSellerOrderProcessingAsync(string ownerUserId, Guid subOrderId);
        Task<bool> MarkSellerOrderDeliveredAsync(string ownerUserId, Guid subOrderId);
        Task<ChatConversationUpdateDto?> GetConversationUpdateForBuyerAsync(string buyerUserId, Guid conversationId);
        Task<ChatConversationUpdateDto?> GetConversationUpdateForSellerAsync(string ownerUserId, Guid conversationId);
    }
}
