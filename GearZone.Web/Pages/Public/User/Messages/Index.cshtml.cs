using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Domain.Entities;
using GearZone.Web.Pages.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace GearZone.Web.Pages.Public.User.Messages
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IChatService _chatService;
        private readonly BuyerInboxComposer _buyerInboxComposer;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            IChatService chatService,
            BuyerInboxComposer buyerInboxComposer,
            UserManager<ApplicationUser> userManager)
        {
            _chatService = chatService;
            _buyerInboxComposer = buyerInboxComposer;
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
        public string? StoreSlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProductSlug { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Redirect("/Public/Auth/Login");
            }

            return RedirectToPage("/Public/User/Profile", new
            {
                tab = "messages",
                conversationId = ConversationId,
                filter = string.Equals(Filter, "all", StringComparison.OrdinalIgnoreCase) ? null : Filter,
                searchTerm = SearchTerm,
                counterpartScopeKey = CounterpartScopeKey,
                storeSlug = StoreSlug,
                productSlug = ProductSlug
            });
        }

        public async Task<IActionResult> OnGetWidgetBootstrapAsync(Guid? conversationId, string? storeSlug, string? productSlug, string? counterpartScopeKey = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var widget = await _chatService.GetBuyerWidgetBootstrapAsync(userId, new ChatWidgetBootstrapQueryDto
            {
                ConversationId = conversationId,
                StoreSlug = storeSlug,
                ProductSlug = productSlug,
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = counterpartScopeKey ?? CounterpartScopeKey,
                LoadedPageCount = 1,
                InboxPageSize = 20,
                MessagePageSize = 30
            });

            if (widget.ActiveThread != null)
            {
                await _chatService.MarkConversationReadAsync(userId, widget.ActiveThread.ConversationId);
            }

            Response.Headers["X-Requested-Target-Unavailable"] = widget.RequestedTargetUnavailable ? "true" : "false";

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatInboxLayout.cshtml",
                ViewData = new ViewDataDictionary<ChatInboxPageViewModel>(ViewData, _buyerInboxComposer.MapWidget(widget, userId, "/messages"))
            };
        }

        public async Task<IActionResult> OnGetConversationListAsync(Guid? conversationId, string surface = "page", int loadedConversationPageCount = 1)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var isWidgetSurface = string.Equals(surface, "widget", StringComparison.OrdinalIgnoreCase);
            var inbox = await _buyerInboxComposer.BuildInboxAsync(userId, new BuyerInboxBuildRequest
            {
                BasePath = "/messages",
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = CounterpartScopeKey,
                ProductSlug = ProductSlug,
                SelectedConversationId = conversationId,
                IncludeThread = false,
                IsWidgetSurface = isWidgetSurface,
                LoadedConversationPageCount = loadedConversationPageCount
            });
            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatConversationList.cshtml",
                ViewData = new ViewDataDictionary<ChatConversationListViewModel>(ViewData, _buyerInboxComposer.BuildConversationListViewModel(inbox))
            };
        }

        public async Task<IActionResult> OnGetThreadAsync(Guid conversationId, int loadedPageCount = 1, string surface = "page", string? productSlug = null)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var isWidgetSurface = string.Equals(surface, "widget", StringComparison.OrdinalIgnoreCase);
            var thread = await _buyerInboxComposer.GetThreadAsync(
                userId,
                conversationId,
                loadedPageCount,
                productSlug ?? ProductSlug);

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatThreadPane.cshtml",
                ViewData = new ViewDataDictionary<ChatThreadPaneViewModel>(ViewData, _buyerInboxComposer.BuildThreadPaneViewModel(userId, thread, isWidgetSurface))
            };
        }
    }
}
