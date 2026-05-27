using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.StoreOwner.Messages
{
    [Authorize(Roles = "Store Owner")]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(IChatService chatService, UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        public ChatInboxPageViewModel Inbox { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? ConversationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CounterpartScopeKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public Guid? SubOrderId { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            ViewData["Title"] = "Customer Messages";
            ViewData["PageHeader"] = "Customer Messages";
            ViewData["ActivePage"] = "Messages";
            ViewData["Breadcrumb"] = new[] { "Messages" };
            ViewData["ContentMode"] = "ChatFullCanvas";

            if (SubOrderId.HasValue && !ConversationId.HasValue)
            {
                var conversationId = await _chatService.EnsureSellerConversationFromSubOrderAsync(userId, SubOrderId.Value);
                if (!conversationId.HasValue)
                {
                    TempData["ErrorMessage"] = "This buyer conversation could not be opened from the selected order.";
                    return RedirectToPage("/StoreOwner/Messages/Index");
                }

                return RedirectToPage(new { conversationId });
            }

            Inbox = await BuildInboxAsync(userId, ConversationId, 1);
            return Page();
        }

        public async Task<IActionResult> OnGetConversationListAsync(Guid? conversationId, int loadedConversationPageCount = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var inbox = await BuildInboxAsync(userId, conversationId, 1, includeThread: false, loadedConversationPageCount: loadedConversationPageCount);
            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatConversationList.cshtml",
                ViewData = new ViewDataDictionary<ChatConversationListViewModel>(ViewData, new ChatConversationListViewModel
                {
                    IsSellerView = true,
                    IsFullCanvasPage = true,
                    CurrentUserId = userId,
                    BasePath = "/StoreOwner/Messages",
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
                })
            };
        }

        public async Task<IActionResult> OnGetThreadAsync(Guid conversationId, int loadedPageCount = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var thread = await _chatService.GetSellerThreadAsync(userId, conversationId, new ChatThreadQueryDto
            {
                LoadedPageCount = loadedPageCount,
                PageSize = 30
            });

            if (thread != null)
            {
                await _chatService.MarkConversationReadAsync(userId, conversationId);
            }

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatThreadPane.cshtml",
                ViewData = new ViewDataDictionary<ChatThreadPaneViewModel>(ViewData, new ChatThreadPaneViewModel
                {
                    IsSellerView = true,
                    IsFullCanvasPage = true,
                    CurrentUserId = userId,
                    EmptyTitle = "Choose a buyer",
                    EmptyDescription = "Pick a buyer from the left column to open the realtime thread.",
                    Thread = thread
                })
            };
        }

        private async Task<ChatInboxPageViewModel> BuildInboxAsync(
            string userId,
            Guid? selectedConversationId,
            int loadedPageCount,
            bool includeThread = true,
            int loadedConversationPageCount = 1)
        {
            var query = new ChatInboxQueryDto
            {
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = CounterpartScopeKey,
                PageNumber = 1,
                PageSize = 20 * Math.Max(1, loadedConversationPageCount)
            };

            var conversations = await _chatService.GetSellerInboxAsync(userId, query);
            var counterpartScopeOptions = await _chatService.GetSellerCounterpartScopeOptionsAsync(userId);
            var activeConversationId = selectedConversationId;
            if (!activeConversationId.HasValue && conversations.Items.Any())
            {
                activeConversationId = conversations.Items[0].ConversationId;
            }

            ChatThreadDto? thread = null;
            if (includeThread && activeConversationId.HasValue)
            {
                thread = await _chatService.GetSellerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                {
                    LoadedPageCount = loadedPageCount,
                    PageSize = 30
                });

                if (thread == null && conversations.Items.Any())
                {
                    activeConversationId = conversations.Items[0].ConversationId;
                    thread = await _chatService.GetSellerThreadAsync(userId, activeConversationId.Value, new ChatThreadQueryDto
                    {
                        LoadedPageCount = 1,
                        PageSize = 30
                    });
                }

                if (thread != null)
                {
                    await _chatService.MarkConversationReadAsync(userId, thread.ConversationId);
                }
            }

            return new ChatInboxPageViewModel
            {
                IsSellerView = true,
                IsFullCanvasPage = true,
                CurrentUserId = userId,
                BasePath = "/StoreOwner/Messages",
                Filter = query.Filter,
                SearchTerm = query.SearchTerm,
                CounterpartScopeKey = query.CounterpartScopeKey,
                ActiveConversationId = activeConversationId,
                TotalUnreadCount = await _chatService.GetSellerUnreadCountAsync(userId),
                LoadedConversationPageCount = Math.Max(1, loadedConversationPageCount),
                EmptyInboxTitle = "No messages yet",
                EmptyInboxDescription = "Customer conversations will appear here as soon as a buyer sends a message.",
                CounterpartScopeOptions = counterpartScopeOptions,
                Conversations = conversations,
                ActiveThread = thread
            };
        }
    }
}

