using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GearZone.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task JoinConversation(Guid conversationId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Authentication is required.");
            }

            var buyerAccess = await _chatService.GetConversationUpdateForBuyerAsync(userId, conversationId);
            var sellerAccess = await _chatService.GetConversationUpdateForSellerAsync(userId, conversationId);
            if (buyerAccess == null && sellerAccess == null)
            {
                throw new HubException("You do not have access to this conversation.");
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationId));
        }

        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetConversationGroupName(conversationId));
        }

        public async Task SendMessage(SendChatMessageDto dto)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Authentication is required.");
            }

            ChatSendMessageResultDto result;
            try
            {
                result = await _chatService.SendMessageAsync(userId, dto);
            }
            catch (InvalidOperationException ex)
            {
                throw new HubException(ex.Message);
            }

            var groupName = GetConversationGroupName(result.ConversationId);
            await Clients.Group(groupName).SendAsync("MessageReceived", result.Message);

            var buyerUpdate = await _chatService.GetConversationUpdateForBuyerAsync(result.BuyerUserId, result.ConversationId);
            if (buyerUpdate != null)
            {
                await Clients.User(result.BuyerUserId).SendAsync("ConversationUpdated", buyerUpdate.Conversation);
                await Clients.User(result.BuyerUserId).SendAsync("UnreadCountsUpdated", new
                {
                    isSellerView = false,
                    totalUnreadCount = buyerUpdate.TotalUnreadCount
                });
            }

            var sellerUpdate = await _chatService.GetConversationUpdateForSellerAsync(result.StoreOwnerUserId, result.ConversationId);
            if (sellerUpdate != null)
            {
                await Clients.User(result.StoreOwnerUserId).SendAsync("ConversationUpdated", sellerUpdate.Conversation);
                await Clients.User(result.StoreOwnerUserId).SendAsync("UnreadCountsUpdated", new
                {
                    isSellerView = true,
                    totalUnreadCount = sellerUpdate.TotalUnreadCount
                });
            }
        }

        public async Task MarkConversationRead(Guid conversationId)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new HubException("Authentication is required.");
            }

            await _chatService.MarkConversationReadAsync(userId, conversationId);

            var buyerUpdate = await _chatService.GetConversationUpdateForBuyerAsync(userId, conversationId);
            if (buyerUpdate != null)
            {
                await Clients.User(userId).SendAsync("ConversationUpdated", buyerUpdate.Conversation);
                await Clients.User(userId).SendAsync("UnreadCountsUpdated", new
                {
                    isSellerView = false,
                    totalUnreadCount = buyerUpdate.TotalUnreadCount
                });
            }

            var sellerUpdate = await _chatService.GetConversationUpdateForSellerAsync(userId, conversationId);
            if (sellerUpdate != null)
            {
                await Clients.User(userId).SendAsync("ConversationUpdated", sellerUpdate.Conversation);
                await Clients.User(userId).SendAsync("UnreadCountsUpdated", new
                {
                    isSellerView = true,
                    totalUnreadCount = sellerUpdate.TotalUnreadCount
                });
            }

            await Clients.Group(GetConversationGroupName(conversationId)).SendAsync("ConversationRead", new
            {
                conversationId,
                readByUserId = userId
            });
        }

        private string? GetCurrentUserId()
        {
            return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        private static string GetConversationGroupName(Guid conversationId)
        {
            return $"conversation:{conversationId}";
        }
    }
}
