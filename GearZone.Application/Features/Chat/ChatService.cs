using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.External;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Chat
{
    public class ChatService : IChatService
    {
        private readonly IConversationRepository _conversationRepository;
        private readonly IChatMessageRepository _chatMessageRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStoreRepository _storeRepository;
        private readonly ISubOrderRepository _subOrderRepository;
        private readonly IOrderStatusHistoryRepository _orderStatusHistoryRepository;
        private readonly IOrderTrackingNotifier _orderTrackingNotifier;
        private readonly IUnitOfWork _unitOfWork;

        public ChatService(
            IConversationRepository conversationRepository,
            IChatMessageRepository chatMessageRepository,
            IProductRepository productRepository,
            IStoreRepository storeRepository,
            ISubOrderRepository subOrderRepository,
            IOrderStatusHistoryRepository orderStatusHistoryRepository,
            IOrderTrackingNotifier orderTrackingNotifier,
            IUnitOfWork unitOfWork)
        {
            _conversationRepository = conversationRepository;
            _chatMessageRepository = chatMessageRepository;
            _productRepository = productRepository;
            _storeRepository = storeRepository;
            _subOrderRepository = subOrderRepository;
            _orderStatusHistoryRepository = orderStatusHistoryRepository;
            _orderTrackingNotifier = orderTrackingNotifier;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<ChatConversationListItemDto>> GetBuyerInboxAsync(string userId, ChatInboxQueryDto query)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new PagedResult<ChatConversationListItemDto>(new List<ChatConversationListItemDto>(), 0, 1, 20);
            }

            NormalizeInboxQuery(query);
            return await _conversationRepository.GetBuyerInboxAsync(userId, query);
        }

        public async Task<PagedResult<ChatConversationListItemDto>> GetSellerInboxAsync(string ownerUserId, ChatInboxQueryDto query)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return new PagedResult<ChatConversationListItemDto>(new List<ChatConversationListItemDto>(), 0, 1, 20);
            }

            NormalizeInboxQuery(query);
            return await _conversationRepository.GetSellerInboxAsync(ownerUserId, query);
        }

        public async Task<List<ChatCounterpartScopeOptionDto>> GetBuyerCounterpartScopeOptionsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new List<ChatCounterpartScopeOptionDto>();
            }

            return await _conversationRepository.GetBuyerCounterpartScopeOptionsAsync(userId);
        }

        public async Task<List<ChatCounterpartScopeOptionDto>> GetSellerCounterpartScopeOptionsAsync(string ownerUserId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return new List<ChatCounterpartScopeOptionDto>();
            }

            return await _conversationRepository.GetSellerCounterpartScopeOptionsAsync(ownerUserId);
        }

        public async Task<ChatThreadDto?> GetBuyerThreadAsync(string userId, Guid conversationId, ChatThreadQueryDto query)
        {
            var conversation = await GetConversationForBuyerAsync(userId, conversationId);
            if (conversation == null)
            {
                return null;
            }

            return await BuildThreadAsync(conversation, userId, false, query);
        }

        public async Task<ChatThreadDto?> GetSellerThreadAsync(string ownerUserId, Guid conversationId, ChatThreadQueryDto query)
        {
            var conversation = await GetConversationForSellerAsync(ownerUserId, conversationId);
            if (conversation == null)
            {
                return null;
            }

            return await BuildThreadAsync(conversation, ownerUserId, true, query);
        }

        public async Task<ChatWidgetBootstrapDto> GetBuyerWidgetBootstrapAsync(string userId, ChatWidgetBootstrapQueryDto query)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return new ChatWidgetBootstrapDto();
            }

            NormalizeWidgetBootstrapQuery(query);

            var activeConversationId = query.ConversationId;
            var requestedTargetUnavailable = false;
            if (!activeConversationId.HasValue && !string.IsNullOrWhiteSpace(query.StoreSlug))
            {
                activeConversationId = await EnsureBuyerConversationAsync(userId, query.StoreSlug);
                requestedTargetUnavailable = !activeConversationId.HasValue;
            }

            var conversations = await _conversationRepository.GetBuyerInboxAsync(userId, new ChatInboxQueryDto
            {
                Filter = query.Filter,
                SearchTerm = query.SearchTerm,
                CounterpartScopeKey = query.CounterpartScopeKey,
                PageNumber = 1,
                PageSize = query.InboxPageSize
            });
            var counterpartOptions = await _conversationRepository.GetBuyerCounterpartScopeOptionsAsync(userId);

            if (!activeConversationId.HasValue && conversations.Items.Any())
            {
                activeConversationId = conversations.Items[0].ConversationId;
            }

            ChatThreadDto? thread = null;
            if (activeConversationId.HasValue)
            {
                thread = await GetBuyerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                {
                    LoadedPageCount = query.LoadedPageCount,
                    PageSize = query.MessagePageSize,
                    ProductSlug = query.ProductSlug
                });

                if (thread == null && conversations.Items.Any())
                {
                    activeConversationId = conversations.Items[0].ConversationId;
                    thread = await GetBuyerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                    {
                        LoadedPageCount = query.LoadedPageCount,
                        PageSize = query.MessagePageSize,
                        ProductSlug = query.ProductSlug
                    });
                }
            }

            return new ChatWidgetBootstrapDto
            {
                ActiveConversationId = activeConversationId,
                Filter = query.Filter,
                SearchTerm = query.SearchTerm,
                CounterpartScopeKey = query.CounterpartScopeKey,
                TotalUnreadCount = await _conversationRepository.GetBuyerUnreadCountAsync(userId),
                RequestedTargetUnavailable = requestedTargetUnavailable,
                CounterpartScopeOptions = counterpartOptions,
                Conversations = conversations,
                ActiveThread = thread
            };
        }

        public async Task<Guid?> EnsureBuyerConversationAsync(string userId, string storeSlug)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(storeSlug))
            {
                return null;
            }

            var store = await _storeRepository.GetBySlugAsync(storeSlug.Trim());
            if (store == null || store.OwnerUserId == userId)
            {
                return null;
            }

            var conversation = await _conversationRepository.GetByBuyerAndStoreAsync(userId, store.Id);
            if (conversation != null)
            {
                return conversation.Id;
            }

            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                BuyerUserId = userId,
                StoreId = store.Id,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();
            return conversation.Id;
        }

        public async Task<Guid?> EnsureSellerConversationFromSubOrderAsync(string ownerUserId, Guid subOrderId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId) || subOrderId == Guid.Empty)
            {
                return null;
            }

            var subOrder = await _subOrderRepository.GetSellerChatSubOrderAsync(ownerUserId, subOrderId);
            if (subOrder == null)
            {
                return null;
            }

            var conversation = await _conversationRepository.GetByBuyerAndStoreAsync(subOrder.Order.UserId, subOrder.StoreId);
            if (conversation != null)
            {
                return conversation.Id;
            }

            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                BuyerUserId = subOrder.Order.UserId,
                StoreId = subOrder.StoreId,
                CreatedAt = DateTime.UtcNow,
                LastMessageAt = DateTime.UtcNow
            };

            await _conversationRepository.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();
            return conversation.Id;
        }

        public async Task<ChatSendMessageResultDto> SendMessageAsync(string userId, SendChatMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new InvalidOperationException("Please login to send a message.");
            }

            if (dto.ConversationId == Guid.Empty)
            {
                throw new InvalidOperationException("Conversation is required.");
            }

            var normalizedContent = string.IsNullOrWhiteSpace(dto.Content)
                ? string.Empty
                : dto.Content.Trim();

            if (string.IsNullOrWhiteSpace(normalizedContent))
            {
                throw new InvalidOperationException("Message cannot be empty.");
            }

            if (normalizedContent.Length > 2000)
            {
                throw new InvalidOperationException("Message cannot exceed 2000 characters.");
            }

            var conversation = await _conversationRepository.GetByIdWithParticipantsAsync(dto.ConversationId);
            if (conversation == null)
            {
                throw new InvalidOperationException("Conversation was not found.");
            }

            var storeOwnerUserId = conversation.Store.OwnerUserId;
            if (conversation.BuyerUserId != userId && storeOwnerUserId != userId)
            {
                throw new InvalidOperationException("You do not have access to this conversation.");
            }

            var utcNow = DateTime.UtcNow;
            conversation.LastMessageAt = utcNow;

            var message = new ChatMessage
            {
                Id = Guid.NewGuid(),
                ConversationId = conversation.Id,
                SenderUserId = userId,
                Content = normalizedContent,
                SentAt = utcNow,
                IsRead = false
            };

            await _chatMessageRepository.AddAsync(message);
            await _conversationRepository.UpdateAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            return new ChatSendMessageResultDto
            {
                ConversationId = conversation.Id,
                BuyerUserId = conversation.BuyerUserId,
                StoreOwnerUserId = storeOwnerUserId,
                Message = new ChatMessageItemDto
                {
                    Id = message.Id,
                    ConversationId = conversation.Id,
                    SenderUserId = userId,
                    SenderDisplayName = userId == storeOwnerUserId
                        ? conversation.Store.StoreName
                        : GetBuyerDisplayName(conversation.BuyerUser),
                    SenderAvatarUrl = userId == storeOwnerUserId
                        ? conversation.Store.LogoUrl
                        : conversation.BuyerUser.AvatarUrl,
                    Content = message.Content,
                    SentAt = message.SentAt,
                    IsRead = message.IsRead,
                    ReadAt = message.ReadAt
                }
            };
        }

        public async Task<int> MarkConversationReadAsync(string userId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(userId) || conversationId == Guid.Empty)
            {
                return 0;
            }

            var conversation = await _conversationRepository.GetByIdWithParticipantsAsync(conversationId);
            if (conversation == null)
            {
                return 0;
            }

            var storeOwnerUserId = conversation.Store.OwnerUserId;
            if (conversation.BuyerUserId != userId && storeOwnerUserId != userId)
            {
                return 0;
            }

            var updatedCount = await _chatMessageRepository.MarkAsReadAsync(conversationId, userId, DateTime.UtcNow);
            if (updatedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync();
            }

            return updatedCount;
        }

        public async Task<int> GetBuyerUnreadCountAsync(string userId)
        {
            return string.IsNullOrWhiteSpace(userId)
                ? 0
                : await _conversationRepository.GetBuyerUnreadCountAsync(userId);
        }

        public async Task<int> GetSellerUnreadCountAsync(string ownerUserId)
        {
            return string.IsNullOrWhiteSpace(ownerUserId)
                ? 0
                : await _conversationRepository.GetSellerUnreadCountAsync(ownerUserId);
        }

        public async Task<PagedResult<SellerChatOrderListItemDto>> GetSellerChatOrdersAsync(string ownerUserId, SellerChatOrderQueryDto query)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return new PagedResult<SellerChatOrderListItemDto>(new List<SellerChatOrderListItemDto>(), 0, 1, 10);
            }

            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 10 : query.PageSize;
            return await _subOrderRepository.GetSellerChatOrdersAsync(ownerUserId, query);
        }

        public async Task<SellerChatOrderDetailDto?> GetSellerChatOrderDetailAsync(string ownerUserId, Guid subOrderId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId) || subOrderId == Guid.Empty)
            {
                return null;
            }

            return await _subOrderRepository.GetSellerChatOrderDetailAsync(ownerUserId, subOrderId);
        }

        public Task<bool> ApproveSellerOrderAsync(string ownerUserId, Guid subOrderId)
        {
            return ChangeSellerOrderStatusAsync(
                ownerUserId,
                subOrderId,
                OrderStatus.Approved,
                "Approved by store owner");
        }

        public Task<bool> RejectSellerOrderAsync(string ownerUserId, Guid subOrderId)
        {
            return ChangeSellerOrderStatusAsync(
                ownerUserId,
                subOrderId,
                OrderStatus.Rejected,
                "Rejected by store owner");
        }

        public Task<bool> MarkSellerOrderProcessingAsync(string ownerUserId, Guid subOrderId)
        {
            return ChangeSellerOrderStatusAsync(
                ownerUserId,
                subOrderId,
                OrderStatus.Processing,
                "Marked as processing by store owner");
        }

        public Task<bool> MarkSellerOrderDeliveredAsync(string ownerUserId, Guid subOrderId)
        {
            return ChangeSellerOrderStatusAsync(
                ownerUserId,
                subOrderId,
                OrderStatus.Delivered,
                "Marked as delivered by store owner");
        }

        public async Task<ChatConversationUpdateDto?> GetConversationUpdateForBuyerAsync(string buyerUserId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(buyerUserId))
            {
                return null;
            }

            var conversation = await _conversationRepository.GetBuyerConversationListItemAsync(buyerUserId, conversationId);
            if (conversation == null)
            {
                return null;
            }

            return new ChatConversationUpdateDto
            {
                IsSellerView = false,
                TotalUnreadCount = await _conversationRepository.GetBuyerUnreadCountAsync(buyerUserId),
                Conversation = conversation
            };
        }

        public async Task<ChatConversationUpdateDto?> GetConversationUpdateForSellerAsync(string ownerUserId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId))
            {
                return null;
            }

            var conversation = await _conversationRepository.GetSellerConversationListItemAsync(ownerUserId, conversationId);
            if (conversation == null)
            {
                return null;
            }

            return new ChatConversationUpdateDto
            {
                IsSellerView = true,
                TotalUnreadCount = await _conversationRepository.GetSellerUnreadCountAsync(ownerUserId),
                Conversation = conversation
            };
        }

        private async Task<Conversation?> GetConversationForBuyerAsync(string userId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(userId) || conversationId == Guid.Empty)
            {
                return null;
            }

            var conversation = await _conversationRepository.GetByIdWithParticipantsAsync(conversationId);
            return conversation?.BuyerUserId == userId ? conversation : null;
        }

        private async Task<Conversation?> GetConversationForSellerAsync(string ownerUserId, Guid conversationId)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId) || conversationId == Guid.Empty)
            {
                return null;
            }

            var conversation = await _conversationRepository.GetByIdWithParticipantsAsync(conversationId);
            return conversation?.Store.OwnerUserId == ownerUserId ? conversation : null;
        }

        private async Task<ChatThreadDto> BuildThreadAsync(
            Conversation conversation,
            string currentUserId,
            bool isSellerView,
            ChatThreadQueryDto query)
        {
            query.LoadedPageCount = query.LoadedPageCount < 1 ? 1 : query.LoadedPageCount;
            query.PageSize = query.PageSize < 1 ? 30 : query.PageSize;

            var take = query.LoadedPageCount * query.PageSize;
            var messages = await _chatMessageRepository.GetRecentMessagesAsync(conversation.Id, take);
            var totalMessages = await _chatMessageRepository.GetMessageCountAsync(conversation.Id);
            var recentOrders = await _subOrderRepository.GetConversationOrderContextAsync(conversation.BuyerUserId, conversation.StoreId, 3);
            var productContext = await GetProductContextAsync(conversation, isSellerView, query.ProductSlug);

            return new ChatThreadDto
            {
                ConversationId = conversation.Id,
                StoreId = conversation.StoreId,
                StoreName = conversation.Store.StoreName,
                StoreSlug = conversation.Store.Slug,
                StoreLogoUrl = conversation.Store.LogoUrl,
                BuyerUserId = conversation.BuyerUserId,
                BuyerDisplayName = GetBuyerDisplayName(conversation.BuyerUser),
                BuyerAvatarUrl = conversation.BuyerUser.AvatarUrl,
                CounterpartName = isSellerView ? GetBuyerDisplayName(conversation.BuyerUser) : conversation.Store.StoreName,
                CounterpartAvatarUrl = isSellerView ? conversation.BuyerUser.AvatarUrl : conversation.Store.LogoUrl,
                IsSellerView = isSellerView,
                LoadedPageCount = query.LoadedPageCount,
                PageSize = query.PageSize,
                HasOlderMessages = totalMessages > take,
                Messages = messages,
                RecentOrders = recentOrders,
                ActiveProductContext = productContext
            };
        }

        private async Task<ChatProductContextDto?> GetProductContextAsync(
            Conversation conversation,
            bool isSellerView,
            string? productSlug)
        {
            if (isSellerView || string.IsNullOrWhiteSpace(productSlug))
            {
                return null;
            }

            var context = await _productRepository.GetChatProductContextBySlugAsync(productSlug.Trim());
            if (context == null || context.StoreId != conversation.StoreId)
            {
                return null;
            }

            return context;
        }

        private static string GetBuyerDisplayName(ApplicationUser buyer)
        {
            if (!string.IsNullOrWhiteSpace(buyer.FullName))
            {
                return buyer.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(buyer.UserName))
            {
                return buyer.UserName.Trim();
            }

            return buyer.Email ?? "Buyer";
        }

        private static void NormalizeInboxQuery(ChatInboxQueryDto query)
        {
            query.PageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            query.PageSize = query.PageSize < 1 ? 20 : query.PageSize;
            query.Filter = string.IsNullOrWhiteSpace(query.Filter)
                ? "all"
                : query.Filter.Trim().ToLowerInvariant();
            if (query.Filter != "unread")
            {
                query.Filter = "all";
            }
            query.SearchTerm = string.IsNullOrWhiteSpace(query.SearchTerm)
                ? null
                : query.SearchTerm.Trim();
            query.CounterpartScopeKey = string.IsNullOrWhiteSpace(query.CounterpartScopeKey)
                ? null
                : query.CounterpartScopeKey.Trim();
        }

        private static void NormalizeWidgetBootstrapQuery(ChatWidgetBootstrapQueryDto query)
        {
            query.LoadedPageCount = query.LoadedPageCount < 1 ? 1 : query.LoadedPageCount;
            query.InboxPageSize = query.InboxPageSize < 1 ? 20 : query.InboxPageSize;
            query.MessagePageSize = query.MessagePageSize < 1 ? 30 : query.MessagePageSize;
            query.Filter = string.IsNullOrWhiteSpace(query.Filter)
                ? "all"
                : query.Filter.Trim().ToLowerInvariant();
            if (query.Filter != "unread")
            {
                query.Filter = "all";
            }
            query.SearchTerm = string.IsNullOrWhiteSpace(query.SearchTerm)
                ? null
                : query.SearchTerm.Trim();
            query.CounterpartScopeKey = string.IsNullOrWhiteSpace(query.CounterpartScopeKey)
                ? null
                : query.CounterpartScopeKey.Trim();
            query.StoreSlug = string.IsNullOrWhiteSpace(query.StoreSlug)
                ? null
                : query.StoreSlug.Trim();
            query.ProductSlug = string.IsNullOrWhiteSpace(query.ProductSlug)
                ? null
                : query.ProductSlug.Trim();
        }

        private async Task<bool> ChangeSellerOrderStatusAsync(
            string ownerUserId,
            Guid subOrderId,
            OrderStatus targetStatus,
            string note)
        {
            if (string.IsNullOrWhiteSpace(ownerUserId) || subOrderId == Guid.Empty)
            {
                return false;
            }

            var subOrder = await _subOrderRepository.Query()
                .Include(x => x.Order)
                .ThenInclude(x => x.Payments)
                .Include(x => x.Order.User)
                .Include(x => x.Store)
                .Include(x => x.Items)
                    .ThenInclude(x => x.Variant)
                        .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == subOrderId && x.Store.OwnerUserId == ownerUserId);
            if (subOrder == null)
            {
                return false;
            }

            var oldStatus = subOrder.Status;
            if (oldStatus == targetStatus)
            {
                return true;
            }

            var isValidTransition =
                (oldStatus == OrderStatus.Pending && (targetStatus == OrderStatus.Approved || targetStatus == OrderStatus.Rejected)) ||
                ((oldStatus == OrderStatus.Approved || oldStatus == OrderStatus.Paid) && targetStatus == OrderStatus.Processing) ||
                (oldStatus == OrderStatus.Processing && targetStatus == OrderStatus.Delivered);

            if (!isValidTransition)
            {
                return false;
            }

            subOrder.Status = targetStatus;
            subOrder.UpdatedAt = DateTime.UtcNow;
            if (targetStatus == OrderStatus.Delivered)
            {
                subOrder.DeliveredAt = DateTime.UtcNow;
                ApplyDeliveredSoldCount(subOrder);
            }

            await _subOrderRepository.UpdateAsync(subOrder);
            await _orderStatusHistoryRepository.AddAsync(new OrderStatusHistory
            {
                Id = Guid.NewGuid(),
                OrderId = subOrder.OrderId,
                OldStatus = oldStatus,
                NewStatus = targetStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedByUserId = ownerUserId,
                Note = note
            });

            await _unitOfWork.SaveChangesAsync();
            await _orderTrackingNotifier.NotifySubOrderUpdatedAsync(subOrder.Id);
            return true;
        }

        private static void ApplyDeliveredSoldCount(SubOrder subOrder)
        {
            if (subOrder.Order?.Payments.Any(p => p.Status == PaymentStatus.Paid) == true)
            {
                return;
            }

            var soldByProduct = subOrder.Items
                .Where(x => x.Variant?.Product != null)
                .GroupBy(x => x.Variant.ProductId)
                .Select(group => new
                {
                    Product = group.First().Variant.Product,
                    Quantity = group.Sum(item => item.Quantity)
                });

            foreach (var item in soldByProduct)
            {
                if (item.Product.IsDeleted)
                {
                    continue;
                }

                item.Product.SoldCount += item.Quantity;
            }
        }
    }
}
