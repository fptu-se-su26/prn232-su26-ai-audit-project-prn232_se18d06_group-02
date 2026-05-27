using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.User.Dtos;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace GearZone.Web.Pages.Checkout
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ICheckoutService _checkoutService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;
        private readonly IVoucherService _voucherService;
        private readonly IShippingService _shippingService;

        public IndexModel(
            ICheckoutService checkoutService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IUserService userService,
            IVoucherService voucherService,
            IShippingService shippingService)
        {
            _checkoutService = checkoutService;
            _orderService = orderService;
            _userManager = userManager;
            _configuration = configuration;
            _userService = userService;
            _voucherService = voucherService;
            _shippingService = shippingService;
        }

        public string? GoongApiKey => _configuration["GOONG_API_KEY"];
        public string? GoongMapKey => _configuration["GOONG_MAP_KEY"];
        public string? MapBoxStyle => _configuration["MAPBOX_STYLE"]; // Fallback if needed

        [BindProperty(SupportsGet = true)]
        public List<Guid> SelectedCartItemIds { get; set; } = new();

        [BindProperty]
        public CheckoutRequestDto CheckoutRequest { get; set; } = new();

        public ApplicationUser CurrentUser { get; set; } = null!;
        public List<CartItem> SelectedItems { get; set; } = new();
        public List<UserAddressDto> UserAddresses { get; set; } = new();
        public decimal GrandTotal { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            CurrentUser = await _userManager.FindByIdAsync(userId);

            if (SelectedCartItemIds == null || !SelectedCartItemIds.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            // Load selected cart items to display via Service
            SelectedItems = await _checkoutService.GetCheckoutItemsAsync(userId, SelectedCartItemIds);

            if (!SelectedItems.Any())
            {
                return RedirectToPage("/Cart/Index");
            }

            GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            // Load user addresses
            UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
            var defaultAddress = UserAddresses.FirstOrDefault(a => a.IsDefault) ?? UserAddresses.FirstOrDefault();

            // Pre-fill shipping info
            CheckoutRequest.ShippingInfo = new ShippingInfoDto
            {
                FullName = defaultAddress?.FullName ?? CurrentUser.FullName ?? string.Empty,
                PhoneNumber = defaultAddress?.PhoneNumber ?? CurrentUser.PhoneNumber ?? string.Empty,
                EmailAddress = CurrentUser.Email ?? string.Empty,
                Address = defaultAddress?.AddressLine ?? string.Empty,
                Ward = defaultAddress?.Ward,
                District = defaultAddress?.District,
                Province = defaultAddress?.Province,
                Latitude = defaultAddress?.Latitude ?? 0,
                Longitude = defaultAddress?.Longitude ?? 0
            };
            CheckoutRequest.CartItemIds = SelectedCartItemIds;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Public/Auth/Login");

            if (CheckoutRequest.CartItemIds == null || CheckoutRequest.CartItemIds.Count == 0)
            {
                TempData["ErrorMessage"] = "Please select at least one item before checkout.";
                return RedirectToPage("/Cart/Index");
            }

            SelectedCartItemIds = CheckoutRequest.CartItemIds;

            // We MUST capture the items before they potentially get cleared from the cart
            SelectedItems = await _checkoutService.GetCheckoutItemsAsync(userId, CheckoutRequest.CartItemIds);
            GrandTotal = SelectedItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            if (!ModelState.IsValid)
            {
                CurrentUser = await _userManager.FindByIdAsync(userId);
                UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
                SelectedCartItemIds = CheckoutRequest.CartItemIds;
                return Page();
            }

            // Use the user's selected payment method
            var result = await _checkoutService.ProcessCheckoutAsync(userId, CheckoutRequest);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Checkout failed.");
                CurrentUser = await _userManager.FindByIdAsync(userId);
                UserAddresses = (await _userService.GetUserAddressesAsync(userId)).ToList();
                SelectedCartItemIds = CheckoutRequest.CartItemIds;
                return Page();
            }

            // If PayOS: redirect to internal QR page
            if (!string.IsNullOrEmpty(result.CheckoutUrl))
            {
                TempData["PayOS_OrderId"] = result.OrderId?.ToString();
                TempData["PayOS_OrderCode"] = result.OrderCode;
                TempData["PayOS_Bin"] = result.Bin;
                TempData["PayOS_AccountNumber"] = result.AccountNumber;
                TempData["PayOS_AccountName"] = result.AccountName;
                TempData["PayOS_Amount"] = result.Amount?.ToString();
                TempData["PayOS_Description"] = result.Description;
                TempData["PayOS_QrCode"] = result.QrCode;
                TempData["PayOS_CheckoutUrl"] = result.CheckoutUrl;
                return RedirectToPage("./PayOSCheckout");
            }

            // If COD: redirect to success page
            return RedirectToPage("./Success", new { orderId = result.OrderId });
        }

        public async Task<IActionResult> OnPostAddNewAddressAsync([FromBody] CreateUserAddressDto request)
        {
            Console.WriteLine("DEBUG: OnPostAddNewAddressAsync called");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Clear page-level validation errors for the AJAX call
            ModelState.Clear();
            if (!TryValidateModel(request))
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                Console.WriteLine($"DEBUG: ModelState Invalid: {string.Join(", ", errors)}");
                return BadRequest(new { errors = errors });
            }

            try 
            {
                var addressId = await _userService.AddAddressAsync(userId, request);
                var address = await _userService.GetAddressByIdAsync(addressId, userId);
                Console.WriteLine("DEBUG: Address saved successfully");
                return new JsonResult(new { success = true, address = address });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG: Exception saving address: {ex.Message}");
                return StatusCode(500, new { message = "Error saving to database", detail = ex.Message });
            }
        }

        public async Task<IActionResult> OnPostCalculateShippingAsync([FromBody] CalculateShippingRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (request.CartItemIds == null || !request.CartItemIds.Any())
                return BadRequest("No items selected.");

            var items = await _checkoutService.GetCheckoutItemsAsync(userId, request.CartItemIds);
            var result = await _shippingService.CalculateShippingFeeAsync(request.Latitude, request.Longitude, items);

            return new JsonResult(result);
        }

        // ─── Voucher AJAX Handlers ────────────────────────

        public async Task<IActionResult> OnPostApplyVoucherAsync([FromBody] ApplyVoucherRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Code))
                return new JsonResult(new { isValid = false, errorMessage = "Please enter a voucher code." });

            var expectedType = request.Type == "shipping"
                ? VoucherType.ShippingDiscount
                : VoucherType.OrderDiscount;

            var result = await _voucherService.ValidateVoucherAsync(
                request.Code, userId, request.MerchandiseTotal, request.ShippingFee, expectedType);

            return new JsonResult(new
            {
                isValid = result.IsValid,
                errorMessage = result.ErrorMessage,
                voucherId = result.VoucherId,
                voucherName = result.VoucherName,
                voucherCode = result.VoucherCode,
                discountAmount = result.DiscountAmount,
                discountLabel = result.DiscountLabel
            });
        }

        public async Task<IActionResult> OnGetAvailableVouchersAsync(string type, decimal merchandiseTotal, decimal shippingFee)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var voucherType = type == "shipping"
                ? VoucherType.ShippingDiscount
                : VoucherType.OrderDiscount;

            var vouchers = await _voucherService.GetAvailableVouchersForCheckoutAsync(
                userId, merchandiseTotal, shippingFee, voucherType);

            return new JsonResult(vouchers);
        }
    }

    public class ApplyVoucherRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = "order"; // "order" or "shipping"
        public decimal MerchandiseTotal { get; set; }
        public decimal ShippingFee { get; set; }
    }

    public class CalculateShippingRequest
    {
        public List<Guid> CartItemIds { get; set; } = new();
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

