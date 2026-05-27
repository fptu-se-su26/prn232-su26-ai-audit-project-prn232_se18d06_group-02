using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;

namespace GearZone.Web.Pages.Shared.Models
{
    public class ChatInboxPageViewModel
    {
        public bool IsSellerView { get; set; }
        public bool IsWidgetSurface { get; set; }
        public bool IsFullCanvasPage { get; set; }
        public bool IsAccountCenterSurface { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public string BasePath { get; set; } = string.Empty;
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public string? ProductSlug { get; set; }
        public Guid? ActiveConversationId { get; set; }
        public int TotalUnreadCount { get; set; }
        public int LoadedConversationPageCount { get; set; } = 1;
        public string EmptyInboxTitle { get; set; } = string.Empty;
        public string EmptyInboxDescription { get; set; } = string.Empty;
        public List<ChatCounterpartScopeOptionDto> CounterpartScopeOptions { get; set; } = new();
        public PagedResult<ChatConversationListItemDto> Conversations { get; set; } = new();
        public ChatThreadDto? ActiveThread { get; set; }
    }

    public class ChatConversationListViewModel
    {
        public bool IsSellerView { get; set; }
        public bool IsWidgetSurface { get; set; }
        public bool IsFullCanvasPage { get; set; }
        public bool IsAccountCenterSurface { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public string BasePath { get; set; } = string.Empty;
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public Guid? ActiveConversationId { get; set; }
        public int TotalUnreadCount { get; set; }
        public int LoadedConversationPageCount { get; set; } = 1;
        public string EmptyInboxTitle { get; set; } = string.Empty;
        public string EmptyInboxDescription { get; set; } = string.Empty;
        public List<ChatCounterpartScopeOptionDto> CounterpartScopeOptions { get; set; } = new();
        public PagedResult<ChatConversationListItemDto> Conversations { get; set; } = new();
    }

    public class ChatThreadPaneViewModel
    {
        public bool IsSellerView { get; set; }
        public bool IsWidgetSurface { get; set; }
        public bool IsFullCanvasPage { get; set; }
        public bool IsAccountCenterSurface { get; set; }
        public string CurrentUserId { get; set; } = string.Empty;
        public string EmptyTitle { get; set; } = string.Empty;
        public string EmptyDescription { get; set; } = string.Empty;
        public ChatThreadDto? Thread { get; set; }
    }

    public class BuyerChatWidgetViewModel
    {
        public bool IsAuthenticated { get; set; }
        public int TotalUnreadCount { get; set; }
        public string BootstrapUrl { get; set; } = "/messages?handler=WidgetBootstrap";
        public string MessagesUrl { get; set; } = "/messages";
    }
}
