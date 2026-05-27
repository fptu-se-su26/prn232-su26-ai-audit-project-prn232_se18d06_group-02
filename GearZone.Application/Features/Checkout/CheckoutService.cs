using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.Payment;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GearZone.Application.Features.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartItemRepository _cartItemRepository;
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentStrategyFactory _paymentStrategyFactory;
        private readonly IBackgroundJobService _backgroundJobService;
        private readonly IUserService _userService;
        private readonly IVoucherService _voucherService;
        private readonly IShippingService _shippingService;

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            PaymentStrategyFactory paymentStrategyFactory,
            IBackgroundJobService backgroundJobService,
            IUserService userService,
            IVoucherService voucherService,
            IShippingService shippingService)
        {
            _cartItemRepository = cartItemRepository;
            _productVariantRepository = productVariantRepository;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _paymentStrategyFactory = paymentStrategyFactory;
            _backgroundJobService = backgroundJobService;
            _userService = userService;
            _voucherService = voucherService;
            _shippingService = shippingService;
        }

        public async Task<CheckoutResponseDto> ProcessCheckoutAsync(
            string userId,
            CheckoutRequestDto request,
            CancellationToken ct = default)
        {
            // 1. Validate input
            if (request.CartItemIds == null || !request.CartItemIds.Any())
                return new CheckoutResponseDto { Success = false, ErrorMessage = "No items selected for checkout." };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "User not found." };

            // 2. Fetch cart items
            var cartItems = await _cartItemRepository.GetCartItemsForCheckoutAsync(
                request.CartItemIds, userId, ct);

            if (cartItems.Count != request.CartItemIds.Count)
                return new CheckoutResponseDto { Success = false, ErrorMessage = "One or more invalid cart items selected." };

            // 3. Validate stock & deduct
            foreach (var cartItem in cartItems)
            {
                if (cartItem.Variant.StockQuantity < cartItem.Quantity)
                    return new CheckoutResponseDto
                    {
                        Success = false,
                        ErrorMessage = $"Insufficient stock for {cartItem.Variant.Product.Name}."
                    };

                cartItem.Variant.StockQuantity -= cartItem.Quantity;
                await _productVariantRepository.UpdateAsync(cartItem.Variant);
            }

            // 4. Calculate shipping fees if coordinates available
            decimal totalShippingFee = 0;
            List<Shipping.Dtos.StoreShippingFeeDto>? storeShippingFees = null;

            if (request.ShippingInfo.Latitude != null && request.ShippingInfo.Longitude != null)
            {
                var shippingResult = await _shippingService.CalculateShippingFeeAsync(
                    (double)request.ShippingInfo.Latitude,
                    (double)request.ShippingInfo.Longitude,
                    cartItems);
                
                totalShippingFee = shippingResult.TotalShippingFee;
                storeShippingFees = shippingResult.StoreFees;
            }

            // 5. Validate vouchers with final checkout numbers
            Guid? orderVoucherId = null;
            decimal orderDiscountAmount = 0;
            Guid? shippingVoucherId = null;
            decimal shippingDiscountAmount = 0;

            var merchandiseTotal = cartItems.Sum(ci => ci.Quantity * ci.Variant.Price);

            if (!string.IsNullOrWhiteSpace(request.OrderVoucherCode))
            {
                var orderVoucherResult = await _voucherService.ValidateVoucherAsync(
                    request.OrderVoucherCode,
                    userId,
                    merchandiseTotal,
                    totalShippingFee,
                    Domain.Enums.VoucherType.OrderDiscount);

                if (!orderVoucherResult.IsValid)
                    return new CheckoutResponseDto { Success = false, ErrorMessage = orderVoucherResult.ErrorMessage };

                orderVoucherId = orderVoucherResult.VoucherId;
                orderDiscountAmount = orderVoucherResult.DiscountAmount;
            }

            if (!string.IsNullOrWhiteSpace(request.ShippingVoucherCode))
            {
                var shippingVoucherResult = await _voucherService.ValidateVoucherAsync(
                    request.ShippingVoucherCode,
                    userId,
                    merchandiseTotal,
                    totalShippingFee,
                    Domain.Enums.VoucherType.ShippingDiscount);

                if (!shippingVoucherResult.IsValid)
                    return new CheckoutResponseDto { Success = false, ErrorMessage = shippingVoucherResult.ErrorMessage };

                shippingVoucherId = shippingVoucherResult.VoucherId;
                shippingDiscountAmount = shippingVoucherResult.DiscountAmount;
            }

            // 6. Create order
            var order = await _orderService.CreateOrderAsync(
                userId, request, cartItems,
                orderVoucherId, orderDiscountAmount,
                shippingVoucherId, shippingDiscountAmount,
                totalShippingFee, storeShippingFees,
                ct);

            // 6. Process payment via Strategy Pattern
            var strategy = _paymentStrategyFactory.GetStrategy(request.PaymentMethod);
            var paymentResult = await strategy.ProcessPaymentAsync(order);

            if (!paymentResult.Success)
            {
                return new CheckoutResponseDto
                {
                    Success = false,
                    ErrorMessage = paymentResult.ErrorMessage ?? "Payment processing failed."
                };
            }

            // 7. Clear cart items
            await _cartService.ClearCartItemsAsync(request.CartItemIds, ct);

            // 8. Record voucher usage
            if (orderVoucherId != null)
                await _voucherService.RecordVoucherUsageAsync((Guid)orderVoucherId, userId, order.Id, orderDiscountAmount);
            if (shippingVoucherId != null)
                await _voucherService.RecordVoucherUsageAsync((Guid)shippingVoucherId, userId, order.Id, shippingDiscountAmount);

            // 7. Save address if requested
            if (request.SaveAddress)
            {
                await _userService.AddAddressAsync(userId, new Features.User.Dtos.CreateUserAddressDto
                {
                    FullName = request.ShippingInfo.FullName,
                    PhoneNumber = request.ShippingInfo.PhoneNumber,
                    AddressLine = request.ShippingInfo.Address,
                    Ward = request.ShippingInfo.Ward,
                    District = request.ShippingInfo.District,
                    Province = request.ShippingInfo.Province,
                    Latitude = request.ShippingInfo.Latitude,
                    Longitude = request.ShippingInfo.Longitude,
                    IsDefault = request.IsDefaultAddress,
                    AddressType = request.ShippingInfo.AddressType
                });
            }

            // 8. Persist all changes
            await _unitOfWork.SaveChangesAsync(ct);

            // 9. Schedule real-time timeout job if PayOS
            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                _backgroundJobService.SchedulePaymentTimeout(order.Id, TimeSpan.FromMinutes(10));
            }

            return new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
                OrderCode = order.OrderCode.ToString(),
                CheckoutUrl = paymentResult.CheckoutUrl,
                Bin = paymentResult.Bin,
                AccountNumber = paymentResult.AccountNumber,
                AccountName = paymentResult.AccountName,
                Amount = paymentResult.Amount,
                Description = paymentResult.Description,
                QrCode = paymentResult.QrCode
            };
        }

        public async Task<List<CartItem>> GetCheckoutItemsAsync(string userId, List<Guid> cartItemIds, CancellationToken ct = default)
        {
            return await _cartItemRepository.Query()
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Images)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(ci => cartItemIds.Contains(ci.Id) && ci.Cart.UserId == userId)
                .ToListAsync(ct);
        }
    }
}
