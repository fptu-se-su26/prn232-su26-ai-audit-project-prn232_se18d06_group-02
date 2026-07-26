using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.User.Dtos;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GearZone.Api.Controllers;

[Authorize]
public class CheckoutController : BaseApiController
{
    private readonly ICheckoutService _checkoutService;
    private readonly IOrderService _orderService;
    private readonly IUserService _userService;
    private readonly IShippingService _shippingService;
    private readonly IPaymentService _paymentService;
    private readonly IBackgroundJobService _backgroundJobService;

    public CheckoutController(
        ICheckoutService checkoutService,
        IOrderService orderService,
        IUserService userService,
        IShippingService shippingService,
        IPaymentService paymentService,
        IBackgroundJobService backgroundJobService)
    {
        _checkoutService = checkoutService;
        _orderService = orderService;
        _userService = userService;
        _shippingService = shippingService;
        _paymentService = paymentService;
        _backgroundJobService = backgroundJobService;
    }

    // GET /api/checkout?cartItemIds=id1&cartItemIds=id2
    [HttpGet]
    public async Task<IActionResult> GetCheckoutData([FromQuery] List<Guid> cartItemIds)
    {
        if (!cartItemIds.Any()) return FailResponse("No cart items selected.");

        var items = await _checkoutService.GetCheckoutItemsAsync(CurrentUserId!, cartItemIds);
        if (!items.Any()) return FailResponse("Selected items not found or are unavailable.");

        var addresses = await _userService.GetUserAddressesAsync(CurrentUserId!);
        var quote = await _checkoutService.GetQuoteAsync(
            CurrentUserId!,
            new CheckoutQuoteRequestDto { CartItemIds = cartItemIds });
        if (!quote.Success)
            return FailResponse(
                quote.ErrorMessage ?? "Unable to calculate checkout quote.",
                quote.IsConflict ? 409 : 400);

        return OkResponse(new
        {
            items,
            addresses,
            grandTotal = quote.MerchandiseSubtotal,
            quote
        });
    }

    // POST /api/checkout
    [HttpPost]
    public async Task<IActionResult> PlaceOrder([FromBody] CheckoutRequestDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var result = await _checkoutService.ProcessCheckoutAsync(CurrentUserId!, request);

        if (!result.Success)
            return FailResponse(
                result.ErrorMessage ?? "Checkout failed.",
                result.IsConflict ? 409 : 400);

        // COD — return order ID directly
        if (string.IsNullOrEmpty(result.CheckoutUrl))
            return OkResponse(new { orderId = result.OrderId, paymentMethod = "COD" }, "Order placed successfully.");

        // PayOS — return QR/payment data for React to render
        return OkResponse(new
        {
            orderId = result.OrderId,
            paymentMethod = "PayOS",
            orderCode = result.OrderCode,
            bin = result.Bin,
            accountNumber = result.AccountNumber,
            accountName = result.AccountName,
            amount = result.Amount,
            description = result.Description,
            qrCode = result.QrCode,
            checkoutUrl = result.CheckoutUrl
        }, "Order created. Complete payment via QR.");
    }

    // POST /api/checkout/quote
    [HttpPost("quote")]
    public async Task<IActionResult> Quote(
        [FromBody] CheckoutQuoteRequestDto request,
        CancellationToken ct)
    {
        if (request.CartItemIds.Count == 0)
            return FailResponse("No cart items selected.");

        var quote = await _checkoutService.GetQuoteAsync(CurrentUserId!, request, ct);
        return quote.Success
            ? OkResponse(quote)
            : FailResponse(
                quote.ErrorMessage ?? "Unable to calculate checkout quote.",
                quote.IsConflict ? 409 : 400);
    }

    // POST /api/checkout/cancel-payment
    [HttpPost("cancel-payment")]
    public async Task<IActionResult> CancelPayment([FromBody] CancelPaymentRequest request)
    {
        var order = await _orderService.GetOrderByIdAsync(request.OrderId);
        if (order == null || !string.Equals(order.UserId, CurrentUserId, StringComparison.Ordinal))
            return FailResponse("Unable to cancel the order.");

        _backgroundJobService.EnqueueOrderCancellation(request.OrderId, CurrentUserId!);
        return OkResponse("Order cancellation queued.");
    }

    // GET /api/checkout/payment-return?orderCode=&status=
    [HttpGet("payment-return")]
    public async Task<IActionResult> PaymentReturn([FromQuery] long? orderCode, [FromQuery] string? status)
    {
        if (orderCode == null) return FailResponse("Invalid payment return parameters.");

        var result = await _paymentService.VerifyAndConfirmPaymentAsync(orderCode.Value);

        if (result.Success && result.OrderId.HasValue)
            return OkResponse(new { orderId = result.OrderId.Value, paid = true });

        return FailResponse(result.ErrorMessage ?? "Payment verification failed.");
    }

    // GET /api/checkout/success/{orderId}
    [HttpGet("success/{orderId:guid}")]
    public async Task<IActionResult> Success(Guid orderId)
    {
        var order = await _orderService.GetOrderByIdAsync(orderId);
        if (order == null || !string.Equals(order.UserId, CurrentUserId, StringComparison.Ordinal))
            return FailResponse("Order not found.", 404);

        return OkResponse(order);
    }

    // POST /api/checkout/addresses
    [HttpPost("addresses")]
    public async Task<IActionResult> AddAddress([FromBody] CreateUserAddressDto request)
    {
        if (!ModelState.IsValid) return ValidationFailResponse();

        var addressId = await _userService.AddAddressAsync(CurrentUserId!, request);
        var address = await _userService.GetAddressByIdAsync(addressId, CurrentUserId!);
        return CreatedResponse(address, $"/api/users/me/addresses/{addressId}");
    }

    // POST /api/checkout/calculate-shipping
    [HttpPost("calculate-shipping")]
    public async Task<IActionResult> CalculateShipping([FromBody] CalculateShippingRequest request)
    {
        if (!request.CartItemIds.Any()) return FailResponse("No items selected.");

        var items = await _checkoutService.GetCheckoutItemsAsync(CurrentUserId!, request.CartItemIds);
        var result = await _shippingService.CalculateShippingFeeAsync(request.Latitude, request.Longitude, items);
        return OkResponse(result);
    }

    // POST /api/checkout/apply-voucher
    [HttpPost("apply-voucher")]
    public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return OkResponse(new { isValid = false, errorMessage = "Please enter a voucher code." });

        if (request.CartItemIds.Count == 0)
            return FailResponse("Cart item IDs are required.");

        var isShipping = request.Type.Equals("shipping", StringComparison.OrdinalIgnoreCase);
        var quote = await _checkoutService.GetQuoteAsync(
            CurrentUserId!,
            new CheckoutQuoteRequestDto
            {
                CartItemIds = request.CartItemIds,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                OrderVoucherCode = isShipping ? null : request.Code,
                ShippingVoucherCode = isShipping ? request.Code : null
            });

        var applied = isShipping ? quote.ShippingVoucher : quote.OrderVoucher;
        if (!quote.Success || applied == null)
        {
            return OkResponse(new
            {
                isValid = false,
                errorMessage = quote.ErrorMessage ?? "Voucher is not eligible."
            });
        }

        return OkResponse(new
        {
            isValid = true,
            voucherId = applied.VoucherId,
            voucherName = applied.Name,
            voucherCode = applied.Code,
            discountAmount = applied.DiscountAmount,
            scope = applied.Scope,
            storeId = applied.StoreId
        });
    }

    // GET /api/checkout/vouchers?type=&cartItemIds=&latitude=&longitude=
    [HttpGet("vouchers")]
    public async Task<IActionResult> AvailableVouchers(
        [FromQuery] string type,
        [FromQuery] List<Guid> cartItemIds,
        [FromQuery] double latitude,
        [FromQuery] double longitude)
    {
        if (cartItemIds.Count == 0)
            return FailResponse("Cart item IDs are required.");

        var quote = await _checkoutService.GetQuoteAsync(
            CurrentUserId!,
            new CheckoutQuoteRequestDto
            {
                CartItemIds = cartItemIds,
                Latitude = latitude,
                Longitude = longitude
            });
        if (!quote.Success)
            return FailResponse(quote.ErrorMessage ?? "Unable to load vouchers.", 409);

        var vouchers = type.Equals("shipping", StringComparison.OrdinalIgnoreCase)
            ? quote.AvailableShippingVouchers
            : quote.AvailableOrderVouchers;
        return OkResponse(vouchers);
    }
}

public class CancelPaymentRequest
{
    public Guid OrderId { get; set; }
}

public class CalculateShippingRequest
{
    public List<Guid> CartItemIds { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

public class ApplyVoucherRequest
{
    public string Code { get; set; } = string.Empty;
    public string Type { get; set; } = "order";
    public List<Guid> CartItemIds { get; set; } = new();
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
