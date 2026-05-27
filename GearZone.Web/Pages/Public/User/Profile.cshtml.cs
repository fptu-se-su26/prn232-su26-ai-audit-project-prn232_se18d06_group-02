using GearZone.Domain.Entities;
using GearZone.Application.Features.Admin.Dtos;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Common.Models;
using GearZone.Application.Features.Chat.Dtos;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Application.Features.Reviews.Dtos;
using GearZone.Web.Pages.Public.User.Messages;
using GearZone.Web.Pages.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Security.Claims;
using GearZone.Application.Features.User.Dtos;

namespace GearZone.Web.Pages.Public.User
{
    public class ProfileModel : PageModel
    {
        private readonly IAuthService _authService;
        private readonly ISellerStoreService _sellerStoreService;
        private readonly IOrderService _orderService;
        private readonly IProductReviewService _productReviewService;
        private readonly BuyerInboxComposer _buyerInboxComposer;
        private readonly IUserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProfileModel> _logger;

        public ProfileModel(
            IAuthService authService,
            ISellerStoreService sellerStoreService,
            IOrderService orderService,
            IProductReviewService productReviewService,
            BuyerInboxComposer buyerInboxComposer,
            IUserService userService,
            IConfiguration configuration,
            ILogger<ProfileModel> logger)
        {
            _authService = authService;
            _sellerStoreService = sellerStoreService;
            _orderService = orderService;
            _productReviewService = productReviewService;
            _buyerInboxComposer = buyerInboxComposer;
            _userService = userService;
            _configuration = configuration;
            _logger = logger;
        }

        public UserDto? CurrentUser { get; set; }
        public string ActiveTab { get; set; } = "orders";
        public Store? UserStore { get; set; }
        public PagedResult<UserOrderDto> Orders { get; set; } = new();
        public UserOrderStatusSummaryDto OrderStatusSummary { get; set; } = new();
        public PagedResult<MyReviewDto> Reviews { get; set; } = new();
        public ChatInboxPageViewModel MessagesInbox { get; set; } = new();
        public IEnumerable<UserAddressDto> UserAddresses { get; set; } = new List<UserAddressDto>();

        [BindProperty]
        public CreateUserAddressDto AddressInput { get; set; } = new();

        [BindProperty]
        public Guid? EditingAddressId { get; set; }

        public string GoongMapKey => _configuration["GOONG_MAP_KEY"] ?? "";

        [BindProperty(SupportsGet = true)]
        public string OrderStatus { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public int OrderPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int ReviewPage { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public Guid? ConversationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; } = "all";

        [BindProperty(SupportsGet = true)]
        public string? CounterpartScopeKey { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StoreSlug { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ProductSlug { get; set; }

        public async Task<IActionResult> OnGetAsync(string? tab = "orders")
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Public/Auth/Login");
            }
            
            UserStore = await _sellerStoreService.GetStoreByOwnerIdAsync(CurrentUser.Id);
            
            ActiveTab = tab?.ToLower() ?? "orders";

            if (ActiveTab == "orders")
            {
                OrderStatusSummary = await _orderService.GetUserOrderStatusSummaryAsync(CurrentUser.Id);
                Orders = await _orderService.GetUserOrdersAsync(CurrentUser.Id, new UserOrderQueryDto
                {
                    Status = OrderStatus,
                    SearchTerm = SearchTerm,
                    PageNumber = OrderPage,
                    PageSize = 5
                });
            }
            else if (ActiveTab == "reviews")
            {
                OrderStatusSummary = await _orderService.GetUserOrderStatusSummaryAsync(CurrentUser.Id);
                Reviews = await _productReviewService.GetMyReviewsAsync(CurrentUser.Id, ReviewPage, 6);
            }
            else if (ActiveTab == "addresses")
            {
                UserAddresses = await _userService.GetUserAddressesAsync(CurrentUser.Id);
            }
            else if (ActiveTab == "messages")
            {
                if (!string.IsNullOrWhiteSpace(StoreSlug) && !ConversationId.HasValue)
                {
                    var ensuredConversationId = await _buyerInboxComposer.EnsureConversationAsync(CurrentUser.Id, StoreSlug);
                    if (!ensuredConversationId.HasValue)
                    {
                        TempData["ErrorMessage"] = "This shop cannot be opened in chat right now.";
                        return RedirectToPage("/Public/User/Profile", new { tab = "messages" });
                    }

                    return RedirectToPage("/Public/User/Profile", new
                    {
                        tab = "messages",
                        conversationId = ensuredConversationId,
                        productSlug = ProductSlug
                    });
                }

                MessagesInbox = await _buyerInboxComposer.BuildInboxAsync(CurrentUser.Id, new BuyerInboxBuildRequest
                {
                    BasePath = GetMessagesBasePath(),
                    Filter = Filter,
                    SearchTerm = SearchTerm,
                    CounterpartScopeKey = CounterpartScopeKey,
                    ProductSlug = ProductSlug,
                    SelectedConversationId = ConversationId,
                    IncludeThread = true,
                    IsAccountCenterSurface = true,
                    LoadedConversationPageCount = 1
                });
            }

            return Page();
        }

        public async Task<IActionResult> OnGetConversationListAsync(Guid? conversationId, int loadedConversationPageCount = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var inbox = await _buyerInboxComposer.BuildInboxAsync(userId, new BuyerInboxBuildRequest
            {
                BasePath = GetMessagesBasePath(),
                Filter = Filter,
                SearchTerm = SearchTerm,
                CounterpartScopeKey = CounterpartScopeKey,
                ProductSlug = ProductSlug,
                SelectedConversationId = conversationId,
                IncludeThread = false,
                IsAccountCenterSurface = true,
                LoadedConversationPageCount = loadedConversationPageCount
            });

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatConversationList.cshtml",
                ViewData = new ViewDataDictionary<ChatConversationListViewModel>(ViewData, _buyerInboxComposer.BuildConversationListViewModel(inbox))
            };
        }

        public async Task<IActionResult> OnGetThreadAsync(Guid conversationId, int loadedPageCount = 1, string? productSlug = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var thread = await _buyerInboxComposer.GetThreadAsync(
                userId,
                conversationId,
                loadedPageCount,
                productSlug ?? ProductSlug);

            return new PartialViewResult
            {
                ViewName = "/Pages/Shared/_ChatThreadPane.cshtml",
                ViewData = new ViewDataDictionary<ChatThreadPaneViewModel>(ViewData, _buyerInboxComposer.BuildThreadPaneViewModel(userId, thread, false, true))
            };
        }

        public async Task<IActionResult> OnPostAddUpdateAddressAsync()
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null) return Unauthorized();

            try
            {
                // Ensure FullName and PhoneNumber are not empty (required by DB)
                if (string.IsNullOrWhiteSpace(AddressInput.FullName))
                    AddressInput.FullName = CurrentUser.FullName ?? CurrentUser.UserName ?? "User";
                if (string.IsNullOrWhiteSpace(AddressInput.PhoneNumber))
                    AddressInput.PhoneNumber = CurrentUser.PhoneNumber ?? "0000000000";

                if (EditingAddressId.HasValue && EditingAddressId != Guid.Empty)
                {
                    await _userService.UpdateAddressAsync(CurrentUser.Id, new UpdateUserAddressDto
                    {
                        Id = EditingAddressId.Value,
                        FullName = AddressInput.FullName,
                        PhoneNumber = AddressInput.PhoneNumber,
                        AddressLine = AddressInput.AddressLine,
                        Ward = AddressInput.Ward,
                        District = AddressInput.District,
                        Province = AddressInput.Province,
                        Latitude = AddressInput.Latitude,
                        Longitude = AddressInput.Longitude,
                        AddressType = AddressInput.AddressType,
                        IsDefault = AddressInput.IsDefault
                    });
                    TempData["SuccessMessage"] = "Address updated successfully.";
                }
                else
                {
                    await _userService.AddAddressAsync(CurrentUser.Id, AddressInput);
                    TempData["SuccessMessage"] = "Address added successfully.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { tab = "addresses" });
        }

        public async Task<IActionResult> OnPostDeleteAddressAsync(Guid addressId)
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null) return Unauthorized();

            try
            {
                await _userService.DeleteAddressAsync(addressId, CurrentUser.Id);
                TempData["SuccessMessage"] = "Address deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { tab = "addresses" });
        }

        public async Task<IActionResult> OnPostSetDefaultAddressAsync(Guid addressId)
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null) return Unauthorized();

            try
            {
                await _userService.SetDefaultAddressAsync(addressId, CurrentUser.Id);
                TempData["SuccessMessage"] = "Default address updated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { tab = "addresses" });
        }

        public async Task<IActionResult> OnGetAddressJsonAsync(Guid addressId)
        {
            CurrentUser = await _authService.GetUserAsync(User);
            if (CurrentUser == null) return Unauthorized();

            var address = await _userService.GetAddressByIdAsync(addressId, CurrentUser.Id);
            if (address == null) return NotFound();

            return new JsonResult(address);
        }

        private string GetMessagesBasePath()
        {
            return Url.Page("/Public/User/Profile", new { tab = "messages" }) ?? "/Public/User/Profile?tab=messages";
        }
    }
}
