using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Web.Pages.Shared.Models;

namespace GearZone.Web.Pages.Public.User.Messages
{
    public sealed class BuyerInboxComposer
    {
        private const string DefaultEmptyTitle = "Choose a conversation";
        private const string DefaultEmptyDescription = "Pick a shop from the left column to view the full thread.";
        private readonly IChatService _chatService;

        public BuyerInboxComposer(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<Guid?> EnsureConversationAsync(string userId, string? storeSlug)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(storeSlug))
            {
                return null;
            }

            return await _chatService.EnsureBuyerConversationAsync(userId, storeSlug);
        }

        public async Task<ChatInboxPageViewModel> BuildInboxAsync(string userId, BuyerInboxBuildRequest request)
        {
            var query = new ChatInboxQueryDto
            {
                Filter = string.IsNullOrWhiteSpace(request.Filter) ? "all" : request.Filter,
                SearchTerm = request.SearchTerm,
                CounterpartScopeKey = request.CounterpartScopeKey,
                PageNumber = 1,
                PageSize = Math.Max(1, request.InboxPageSize) * Math.Max(1, request.LoadedConversationPageCount)
            };

            var conversations = await _chatService.GetBuyerInboxAsync(userId, query);
            var counterpartScopeOptions = await _chatService.GetBuyerCounterpartScopeOptionsAsync(userId);
            var activeConversationId = request.SelectedConversationId;
            if (!activeConversationId.HasValue && conversations.Items.Any())
            {
                activeConversationId = conversations.Items[0].ConversationId;
            }

            ChatThreadDto? thread = null;
            if (request.IncludeThread && activeConversationId.HasValue)
            {
                thread = await GetThreadAsync(
                    userId,
                    activeConversationId.Value,
                    request.LoadedPageCount,
                    request.ProductSlug,
                    request.MessagePageSize,
                    markRead: false);

                if (thread == null && conversations.Items.Any())
                {
                    activeConversationId = conversations.Items[0].ConversationId;
                    thread = await GetThreadAsync(
                        userId,
                        activeConversationId.Value,
                        1,
                        request.ProductSlug,
                        request.MessagePageSize,
                        markRead: false);
                }

                if (thread != null)
                {
                    await _chatService.MarkConversationReadAsync(userId, thread.ConversationId);
                }
            }

            return new ChatInboxPageViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = request.IsWidgetSurface,
                IsAccountCenterSurface = request.IsAccountCenterSurface,
                CurrentUserId = userId,
                BasePath = request.BasePath,
                Filter = query.Filter,
                SearchTerm = query.SearchTerm,
                CounterpartScopeKey = query.CounterpartScopeKey,
                ProductSlug = request.ProductSlug,
                ActiveConversationId = activeConversationId,
                TotalUnreadCount = await _chatService.GetBuyerUnreadCountAsync(userId),
                LoadedConversationPageCount = Math.Max(1, request.LoadedConversationPageCount),
                EmptyInboxTitle = "No conversations yet",
                EmptyInboxDescription = "Open any available shop and press Chat to start a conversation.",
                CounterpartScopeOptions = counterpartScopeOptions,
                Conversations = conversations,
                ActiveThread = thread
            };
        }

        public async Task<ChatThreadDto?> GetThreadAsync(
            string userId,
            Guid conversationId,
            int loadedPageCount,
            string? productSlug,
            int messagePageSize = 30,
            bool markRead = true)
        {
            var thread = await _chatService.GetBuyerThreadAsync(userId, conversationId, new ChatThreadQueryDto
            {
                LoadedPageCount = loadedPageCount,
                PageSize = messagePageSize,
                ProductSlug = productSlug
            });

            if (thread != null && markRead)
            {
                await _chatService.MarkConversationReadAsync(userId, conversationId);
            }

            return thread;
        }

        public ChatInboxPageViewModel MapWidget(ChatWidgetBootstrapDto widget, string userId, string basePath)
        {
            return new ChatInboxPageViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = true,
                IsAccountCenterSurface = false,
                CurrentUserId = userId,
                BasePath = basePath,
                Filter = widget.Filter,
                SearchTerm = widget.SearchTerm,
                CounterpartScopeKey = widget.CounterpartScopeKey,
                ProductSlug = widget.ActiveThread?.ActiveProductContext?.ProductSlug,
                ActiveConversationId = widget.ActiveConversationId,
                TotalUnreadCount = widget.TotalUnreadCount,
                LoadedConversationPageCount = 1,
                EmptyInboxTitle = "No conversations yet",
                EmptyInboxDescription = "Open any available shop and press Chat to start a conversation.",
                CounterpartScopeOptions = widget.CounterpartScopeOptions,
                Conversations = widget.Conversations,
                ActiveThread = widget.ActiveThread
            };
        }

        public ChatConversationListViewModel BuildConversationListViewModel(ChatInboxPageViewModel inbox)
        {
            return new ChatConversationListViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = inbox.IsWidgetSurface,
                IsFullCanvasPage = inbox.IsFullCanvasPage,
                IsAccountCenterSurface = inbox.IsAccountCenterSurface,
                CurrentUserId = inbox.CurrentUserId,
                BasePath = inbox.BasePath,
                Filter = inbox.Filter,
                SearchTerm = inbox.SearchTerm,
                CounterpartScopeKey = inbox.CounterpartScopeKey,
                ActiveConversationId = inbox.ActiveConversationId,
                TotalUnreadCount = inbox.TotalUnreadCount,
                LoadedConversationPageCount = inbox.LoadedConversationPageCount,
                EmptyInboxTitle = inbox.EmptyInboxTitle,
                EmptyInboxDescription = inbox.EmptyInboxDescription,
                CounterpartScopeOptions = inbox.CounterpartScopeOptions,
                Conversations = inbox.Conversations
            };
        }

        public ChatThreadPaneViewModel BuildThreadPaneViewModel(
            string userId,
            ChatThreadDto? thread,
            bool isWidgetSurface,
            bool isAccountCenterSurface = false)
        {
            return new ChatThreadPaneViewModel
            {
                IsSellerView = false,
                IsWidgetSurface = isWidgetSurface,
                IsAccountCenterSurface = isAccountCenterSurface,
                CurrentUserId = userId,
                EmptyTitle = DefaultEmptyTitle,
                EmptyDescription = DefaultEmptyDescription,
                Thread = thread
            };
        }
    }

    public sealed class BuyerInboxBuildRequest
    {
        public string BasePath { get; set; } = "/messages";
        public string Filter { get; set; } = "all";
        public string? SearchTerm { get; set; }
        public string? CounterpartScopeKey { get; set; }
        public string? ProductSlug { get; set; }
        public Guid? SelectedConversationId { get; set; }
        public bool IncludeThread { get; set; } = true;
        public bool IsWidgetSurface { get; set; }
        public bool IsAccountCenterSurface { get; set; }
        public int LoadedPageCount { get; set; } = 1;
        public int LoadedConversationPageCount { get; set; } = 1;
        public int InboxPageSize { get; set; } = 20;
        public int MessagePageSize { get; set; } = 30;
    }
}
