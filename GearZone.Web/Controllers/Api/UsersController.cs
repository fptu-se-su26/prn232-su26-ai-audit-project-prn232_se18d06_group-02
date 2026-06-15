using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Orders.Dtos;
using GearZone.Application.Features.User.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Web.Controllers.Api;

[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IOrderService _orderService;
    private readonly IProductReviewService _reviewService;
    private readonly ISellerStoreService _sellerStoreService;

    public UsersController(
        IUserService userService,
        IAuthService authService,
        IOrderService orderService,
        IProductReviewService reviewService,
        ISellerStoreService sellerStoreService)
    {
        _userService = userService;
        _authService = authService;
        _orderService = orderService;
        _reviewService = reviewService;
        _sellerStoreService = sellerStoreService;
    }

    // GET /api/users/me
    [HttpGet("me")]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("User not found.", 404);
        return OkResponse(user);
    }

    // PUT /api/users/me
    [HttpPut("me")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        await _userService.UpdateProfileAsync(CurrentUserId!, request);
        var user = await _userService.GetProfileAsync(CurrentUserId!);
        return OkResponse(user, "Profile updated.");
    }

    // POST /api/users/me/change-password
    [HttpPost("me/change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
            return FailResponse("Current password and new password are required.");

        if (request.NewPassword != request.ConfirmPassword)
            return FailResponse("Password confirmation does not match.");

        var changed = await _userService.ChangePasswordAsync(CurrentUserId!, request.CurrentPassword, request.NewPassword);
        if (!changed) return FailResponse("Current password is incorrect or the new password is invalid.");

        return OkResponse("Password updated.");
    }

    // GET /api/users/me/store
    [HttpGet("me/store")]
    public async Task<IActionResult> GetMyStore()
    {
        var store = await _sellerStoreService.GetStoreByOwnerIdAsync(CurrentUserId!);
        return OkResponse(store);
    }

    // GET /api/users/me/orders?status=&search=&page=
    [HttpGet("me/orders")]
    public async Task<IActionResult> GetOrders([FromQuery] UserOrderQueryDto query)
    {
        if (query.PageNumber < 1) query.PageNumber = 1;
        if (query.PageSize <= 0) query.PageSize = 5;

        var summary = await _orderService.GetUserOrderStatusSummaryAsync(CurrentUserId!);
        var orders = await _orderService.GetUserOrdersAsync(CurrentUserId!, query);
        return OkResponse(new { summary, orders });
    }

    // GET /api/users/me/reviews?page=
    [HttpGet("me/reviews")]
    public async Task<IActionResult> GetReviews([FromQuery] int page = 1)
    {
        var reviews = await _reviewService.GetMyReviewsAsync(CurrentUserId!, page, 6);
        return OkResponse(reviews);
    }

    // GET /api/users/me/addresses
    [HttpGet("me/addresses")]
    public async Task<IActionResult> GetAddresses()
    {
        var addresses = await _userService.GetUserAddressesAsync(CurrentUserId!);
        return OkResponse(addresses);
    }

    // POST /api/users/me/addresses
    [HttpPost("me/addresses")]
    public async Task<IActionResult> AddAddress([FromBody] CreateUserAddressDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var user = await _authService.GetUserAsync(User);
        if (user == null) return FailResponse("User not found.", 401);

        if (string.IsNullOrWhiteSpace(request.FullName)) request.FullName = user.FullName ?? user.UserName ?? "User";
        if (string.IsNullOrWhiteSpace(request.PhoneNumber)) request.PhoneNumber = user.PhoneNumber ?? "0000000000";

        var addressId = await _userService.AddAddressAsync(CurrentUserId!, request);
        var address = await _userService.GetAddressByIdAsync(addressId, CurrentUserId!);
        return CreatedResponse(address, $"/api/users/me/addresses/{addressId}");
    }

    // GET /api/users/me/addresses/{addressId}
    [HttpGet("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> GetAddress(Guid addressId)
    {
        var address = await _userService.GetAddressByIdAsync(addressId, CurrentUserId!);
        if (address == null) return FailResponse("Address not found.", 404);
        return OkResponse(address);
    }

    // PUT /api/users/me/addresses/{addressId}
    [HttpPut("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(Guid addressId, [FromBody] UpdateUserAddressDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        request.Id = addressId;
        await _userService.UpdateAddressAsync(CurrentUserId!, request);
        return OkResponse("Address updated.");
    }

    // DELETE /api/users/me/addresses/{addressId}
    [HttpDelete("me/addresses/{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        await _userService.DeleteAddressAsync(addressId, CurrentUserId!);
        return OkResponse("Address deleted.");
    }

    // PATCH /api/users/me/addresses/{addressId}/set-default
    [HttpPatch("me/addresses/{addressId:guid}/set-default")]
    public async Task<IActionResult> SetDefaultAddress(Guid addressId)
    {
        await _userService.SetDefaultAddressAsync(addressId, CurrentUserId!);
        return OkResponse("Default address updated.");
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
