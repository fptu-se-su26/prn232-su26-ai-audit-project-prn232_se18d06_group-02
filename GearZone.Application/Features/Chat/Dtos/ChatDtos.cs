using GearZone.Application.Common.Models;
using GearZone.Domain.Enums;

namespace GearZone.Application.Features.Chat.Dtos
{
    public class ChatInboxQueryDto
    {
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class ChatThreadQueryDto
    {
        public int LoadedPageCount { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public string? ProductSlug { get; set; }
    }

    public class ChatWidgetBootstrapQueryDto
    {
        public Guid? ConversationId { get; set; }
        public string? StoreSlug { get; set; }
        public string? ProductSlug { get; set; }
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int LoadedPageCount { get; set; } = 1;
        public int InboxPageSize { get; set; } = 20;
        public int MessagePageSize { get; set; } = 30;
    }

    public class SellerChatOrderQueryDto
    {
        public string? SearchTerm { get; set; }
        public OrderStatus? Status { get; set; }
        public decimal? MinSubtotal { get; set; }
        public decimal? MaxSubtotal { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? SortBy { get; set; } = "createdAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class SendChatMessageDto
    {
        public Guid ConversationId { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public class ChatConversationListItemDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string? StoreLogoUrl { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public string CounterpartName { get; set; } = string.Empty;
        public string? CounterpartAvatarUrl { get; set; }
        public string CounterpartSubtitle { get; set; } = string.Empty;
        public string LastMessagePreview { get; set; } = string.Empty;
        public string LastMessageSenderUserId { get; set; } = string.Empty;
        public DateTime LastMessageAt { get; set; }
        public int UnreadCount { get; set; }
        public bool HasMessages { get; set; }
    }

    public class ChatCounterpartScopeOptionDto
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? AvatarUrl { get; set; }
    }

    public class ChatMessageItemDto
    {
        public Guid Id { get; set; }
        public Guid ConversationId { get; set; }
        public string SenderUserId { get; set; } = string.Empty;
        public string SenderDisplayName { get; set; } = string.Empty;
        public string? SenderAvatarUrl { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
    }

    public class ChatContextOrderDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public string ProductPreview { get; set; } = string.Empty;
    }

    public class ChatProductContextDto
    {
        public Guid ProductId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public string? StoreLogoUrl { get; set; }
        public decimal Price { get; set; }
        public bool IsInStock { get; set; }
    }

    public class SellerChatOrderListItemDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public int ItemCount { get; set; }
        public string ProductPreview { get; set; } = string.Empty;
    }

    public class SellerChatOrderDetailDto
    {
        public Guid SubOrderId { get; set; }
        public long OrderCode { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public string? BuyerEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal CommissionRateSnapshot { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string? ShippingProvider { get; set; }
        public string? TrackingNumber { get; set; }
        public List<SellerChatOrderItemDetailDto> Items { get; set; } = new();
        public List<SellerChatOrderStatusHistoryDto> StatusHistory { get; set; } = new();
    }

    public class SellerChatOrderItemDetailDto
    {
        public Guid OrderItemId { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class SellerChatOrderStatusHistoryDto
    {
        public DateTime ChangedAt { get; set; }
        public OrderStatus? OldStatus { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string ChangedByDisplayName { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class ChatThreadDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public string StoreSlug { get; set; } = string.Empty;
        public string? StoreLogoUrl { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string BuyerDisplayName { get; set; } = string.Empty;
        public string? BuyerAvatarUrl { get; set; }
        public string CounterpartName { get; set; } = string.Empty;
        public string? CounterpartAvatarUrl { get; set; }
        public bool IsSellerView { get; set; }
        public int LoadedPageCount { get; set; }
        public int PageSize { get; set; }
        public bool HasOlderMessages { get; set; }
        public List<ChatMessageItemDto> Messages { get; set; } = new();
        public List<ChatContextOrderDto> RecentOrders { get; set; } = new();
        public ChatProductContextDto? ActiveProductContext { get; set; }
    }

    public class ChatSendMessageResultDto
    {
        public Guid ConversationId { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string StoreOwnerUserId { get; set; } = string.Empty;
        public ChatMessageItemDto Message { get; set; } = new();
    }

    public class ChatConversationUpdateDto
    {
        public bool IsSellerView { get; set; }
        public int TotalUnreadCount { get; set; }
        public ChatConversationListItemDto Conversation { get; set; } = new();
    }

    public class ChatBootstrapResultDto
    {
        public Guid ConversationId { get; set; }
        public Guid StoreId { get; set; }
        public string BuyerUserId { get; set; } = string.Empty;
        public string StoreOwnerUserId { get; set; } = string.Empty;
    }

    public class ChatWidgetBootstrapDto
    {
        public Guid? ActiveConversationId { get; set; }
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public int TotalUnreadCount { get; set; }
        public bool RequestedTargetUnavailable { get; set; }
        public List<ChatCounterpartScopeOptionDto> CounterpartScopeOptions { get; set; } = new();
        public PagedResult<ChatConversationListItemDto> Conversations { get; set; } = new PagedResult<ChatConversationListItemDto>(new List<ChatConversationListItemDto>(), 0, 1, 20);
        public ChatThreadDto? ActiveThread { get; set; }
    }
}
