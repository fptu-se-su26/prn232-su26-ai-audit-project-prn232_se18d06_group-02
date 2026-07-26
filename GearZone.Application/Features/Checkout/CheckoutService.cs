using GearZone.Application.Abstractions.Persistence;
using GearZone.Application.Abstractions.Services;
using GearZone.Application.Features.Checkout.Dtos;
using GearZone.Application.Features.Payment;
using GearZone.Application.Features.Promotions;
using GearZone.Domain.Entities;
using GearZone.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearZone.Application.Features.Checkout
{
    public class CheckoutService : ICheckoutService
    {
        private readonly ICartItemRepository _cartItems;
        private readonly IProductVariantRepository _variants;
        private readonly IOrderRepository _orders;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly PaymentStrategyFactory _paymentStrategies;
        private readonly IBackgroundJobService _backgroundJobs;
        private readonly IUserService _users;
        private readonly IVoucherService _vouchers;
        private readonly IShippingService _shipping;
        private readonly IPromotionPricingService _pricing;
        private readonly IPromotionLifecycleService _promotionLifecycle;

        public CheckoutService(
            ICartItemRepository cartItemRepository,
            IProductVariantRepository productVariantRepository,
            IOrderRepository orderRepository,
            IOrderService orderService,
            ICartService cartService,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            PaymentStrategyFactory paymentStrategyFactory,
            IBackgroundJobService backgroundJobService,
            IUserService userService,
            IVoucherService voucherService,
            IShippingService shippingService,
            IPromotionPricingService pricing,
            IPromotionLifecycleService promotionLifecycle)
        {
            _cartItems = cartItemRepository;
            _variants = productVariantRepository;
            _orders = orderRepository;
            _orderService = orderService;
            _cartService = cartService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _paymentStrategies = paymentStrategyFactory;
            _backgroundJobs = backgroundJobService;
            _users = userService;
            _vouchers = voucherService;
            _shipping = shippingService;
            _pricing = pricing;
            _promotionLifecycle = promotionLifecycle;
        }

        public async Task<CheckoutResponseDto> ProcessCheckoutAsync(
            string userId,
            CheckoutRequestDto request,
            CancellationToken ct = default)
        {
            if (request.CartItemIds == null || request.CartItemIds.Count == 0)
            {
                return Fail("No items selected for checkout.");
            }

            if (request.ShippingInfo == null)
            {
                return Fail("Shipping information is required.");
            }

            if (request.RequestId == Guid.Empty)
            {
                request.RequestId = Guid.NewGuid();
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Fail("User not found.");
            }

            var existing = await _orders.Query()
                .AsNoTracking()
                .Include(x => x.Payments)
                .Include(x => x.StatusHistories)
                .FirstOrDefaultAsync(
                    x => x.UserId == userId && x.CheckoutRequestId == request.RequestId,
                    ct);
            if (existing != null)
            {
                return FromExisting(existing);
            }

            Order order;
            CheckoutQuoteDto quote;
            try
            {
                (order, quote) = await _unitOfWork.ExecuteInTransactionAsync(
                    async transactionCt =>
                    {
                        var transactionItems = await GetCheckoutItemsAsync(
                            userId,
                            request.CartItemIds,
                            transactionCt);
                        if (transactionItems.Count != request.CartItemIds.Distinct().Count())
                        {
                            throw new InvalidOperationException(
                                "One or more selected cart items are unavailable.");
                        }

                        quote = await BuildQuoteAsync(
                            userId,
                            transactionItems,
                            new CheckoutQuoteRequestDto
                            {
                                CartItemIds = request.CartItemIds,
                                Latitude = request.ShippingInfo.Latitude,
                                Longitude = request.ShippingInfo.Longitude,
                                OrderVoucherCode = request.OrderVoucherCode,
                                ShippingVoucherCode = request.ShippingVoucherCode
                            },
                            transactionCt);

                        if (!quote.Success)
                        {
                            throw new InvalidOperationException(
                                quote.ErrorMessage ?? "Checkout quote is no longer valid.");
                        }

                        foreach (var item in transactionItems)
                        {
                            if (!await _variants.TryReserveStockAsync(
                                    item.VariantId,
                                    item.Quantity,
                                    transactionCt))
                            {
                                throw new InvalidOperationException(
                                    $"Insufficient stock for {item.Variant.Product.Name}.");
                            }
                        }

                        var createdOrder = await _orderService.CreateOrderAsync(
                            userId,
                            request,
                            transactionItems,
                            quote,
                            transactionCt);

                        await _promotionLifecycle.ReserveForOrderAsync(
                            createdOrder,
                            transactionCt);

                        if (quote.OrderVoucher != null &&
                            !await _vouchers.ReserveVoucherUsageAsync(
                                quote.OrderVoucher.VoucherId,
                                userId,
                                createdOrder.Id,
                                quote.OrderVoucher.DiscountAmount,
                                transactionCt))
                        {
                            throw new InvalidOperationException(
                                "The order voucher is no longer available.");
                        }

                        if (quote.ShippingVoucher != null &&
                            !await _vouchers.ReserveVoucherUsageAsync(
                                quote.ShippingVoucher.VoucherId,
                                userId,
                                createdOrder.Id,
                                quote.ShippingVoucher.DiscountAmount,
                                transactionCt))
                        {
                            throw new InvalidOperationException(
                                "The shipping voucher is no longer available.");
                        }

                        await _unitOfWork.SaveChangesAsync(transactionCt);
                        return (createdOrder, quote);
                    },
                    ct);
            }
            catch (PromotionQuotaExceededException ex)
            {
                return Fail(ex.Message, true);
            }
            catch (DbUpdateException)
            {
                var duplicate = await _orders.Query()
                    .AsNoTracking()
                    .Include(x => x.Payments)
                    .Include(x => x.StatusHistories)
                    .FirstOrDefaultAsync(
                        x => x.UserId == userId &&
                             x.CheckoutRequestId == request.RequestId,
                        ct);
                return duplicate != null
                    ? FromExisting(duplicate)
                    : Fail(
                        "Checkout conflicted with another request. Please refresh and try again.",
                        true);
            }
            catch (InvalidOperationException ex)
            {
                return Fail(ex.Message, true);
            }

            var paymentResult = await _paymentStrategies
                .GetStrategy(request.PaymentMethod)
                .ProcessPaymentAsync(order);

            if (!paymentResult.Success)
            {
                await _orderService.CancelOrderAsync(order.Id, userId, ct);
                return Fail(paymentResult.ErrorMessage ?? "Payment processing failed.");
            }

            // Persist the payment record and clear purchased cart lines atomically.
            // Cart clearing is set-based/idempotent, so a repeated request cannot
            // fail with "expected 1 row, affected 0 rows".
            await _unitOfWork.ExecuteInTransactionAsync(
                async transactionCt =>
                {
                    await _cartService.ClearCartItemsAsync(
                        request.CartItemIds,
                        transactionCt);
                    await _unitOfWork.SaveChangesAsync(transactionCt);
                    return true;
                },
                ct);

            if (request.SaveAddress)
            {
                await _users.AddAddressAsync(
                    userId,
                    new Features.User.Dtos.CreateUserAddressDto
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

            if (request.PaymentMethod == PaymentMethod.PayOS)
            {
                _backgroundJobs.SchedulePaymentTimeout(order.Id, TimeSpan.FromMinutes(10));
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

        public async Task<CheckoutQuoteDto> GetQuoteAsync(
            string userId,
            CheckoutQuoteRequestDto request,
            CancellationToken ct = default)
        {
            if (request.CartItemIds == null || request.CartItemIds.Count == 0)
            {
                return new CheckoutQuoteDto
                {
                    Success = false,
                    ErrorMessage = "No items selected for checkout."
                };
            }

            var items = await GetCheckoutItemsAsync(userId, request.CartItemIds, ct);
            if (items.Count != request.CartItemIds.Distinct().Count())
            {
                return new CheckoutQuoteDto
                {
                    Success = false,
                    IsConflict = true,
                    ErrorMessage = "One or more selected cart items are unavailable."
                };
            }

            return await BuildQuoteAsync(userId, items, request, ct);
        }

        public async Task<List<CartItem>> GetCheckoutItemsAsync(
            string userId,
            List<Guid> cartItemIds,
            CancellationToken ct = default)
        {
            return await _cartItems.Query()
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Store)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Images)
                .Include(ci => ci.Variant)
                    .ThenInclude(v => v.AttributeValues)
                        .ThenInclude(av => av.CategoryAttributeOption)
                .Where(ci =>
                    cartItemIds.Contains(ci.Id) &&
                    ci.Cart.UserId == userId &&
                    ci.Variant.IsActive &&
                    !ci.Variant.IsDeleted &&
                    !ci.Variant.Product.IsDeleted &&
                    (ci.Variant.Product.Status == ProductStatus.Active ||
                     ci.Variant.Product.Status == ProductStatus.Approved) &&
                    ci.Variant.Product.Store.Status == StoreStatus.Approved)
                .ToListAsync(ct);
        }

        private async Task<CheckoutQuoteDto> BuildQuoteAsync(
            string userId,
            List<CartItem> items,
            CheckoutQuoteRequestDto request,
            CancellationToken ct)
        {
            var prices = await _pricing.GetPricesAsync(
                items.Select(x => x.Variant).ToArray(),
                null,
                ct);

            var lines = items.Select(item =>
            {
                var price = prices[item.VariantId];
                return new CheckoutQuoteLineDto
                {
                    CartItemId = item.Id,
                    VariantId = item.VariantId,
                    ProductId = item.Variant.ProductId,
                    StoreId = item.Variant.Product.StoreId,
                    CategoryId = item.Variant.Product.CategoryId,
                    ProductName = item.Variant.Product.Name,
                    Quantity = item.Quantity,
                    OriginalUnitPrice = price.OriginalPrice,
                    EffectiveUnitPrice = price.EffectivePrice,
                    PromotionDiscountAmount = price.DiscountPerUnit * item.Quantity,
                    PromotionCampaignId = price.CampaignId,
                    PromotionName = price.CampaignName,
                    PromotionEndAt = price.CampaignEndAt,
                    LineTotal = price.EffectivePrice * item.Quantity
                };
            }).ToList();

            var shippingResult =
                request.Latitude != 0 || request.Longitude != 0
                    ? await _shipping.CalculateShippingFeeAsync(
                        request.Latitude,
                        request.Longitude,
                        items)
                    : new Features.Shipping.Dtos.ShippingFeeCalculationResponseDto();

            var context = new VoucherEvaluationContextDto
            {
                Lines = lines.Select(x => new VoucherEvaluationLineDto
                {
                    StoreId = x.StoreId,
                    CategoryId = x.CategoryId,
                    EffectiveSubtotal = x.LineTotal
                }).ToList(),
                ShippingFees = shippingResult.StoreFees.Select(x => new VoucherShippingFeeDto
                {
                    StoreId = x.StoreId,
                    ShippingFee = x.ShippingFee
                }).ToList()
            };

            VoucherValidationResult? orderVoucher = null;
            if (!string.IsNullOrWhiteSpace(request.OrderVoucherCode))
            {
                orderVoucher = await _vouchers.ValidateVoucherForContextAsync(
                    request.OrderVoucherCode,
                    userId,
                    context,
                    VoucherType.OrderDiscount,
                    ct);
                if (!orderVoucher.IsValid)
                {
                    return new CheckoutQuoteDto
                    {
                        Success = false,
                        ErrorMessage = orderVoucher.ErrorMessage
                    };
                }
            }

            VoucherValidationResult? shippingVoucher = null;
            if (!string.IsNullOrWhiteSpace(request.ShippingVoucherCode))
            {
                shippingVoucher = await _vouchers.ValidateVoucherForContextAsync(
                    request.ShippingVoucherCode,
                    userId,
                    context,
                    VoucherType.ShippingDiscount,
                    ct);
                if (!shippingVoucher.IsValid)
                {
                    return new CheckoutQuoteDto
                    {
                        Success = false,
                        ErrorMessage = shippingVoucher.ErrorMessage
                    };
                }
            }

            var merchandiseBeforePromotion =
                lines.Sum(x => x.OriginalUnitPrice * x.Quantity);
            var merchandise = lines.Sum(x => x.LineTotal);
            var orderDiscount = orderVoucher?.DiscountAmount ?? 0;
            var shippingDiscount = shippingVoucher?.DiscountAmount ?? 0;

            return new CheckoutQuoteDto
            {
                Success = true,
                Lines = lines,
                MerchandiseSubtotalBeforePromotion = merchandiseBeforePromotion,
                PromotionDiscountAmount = merchandiseBeforePromotion - merchandise,
                MerchandiseSubtotal = merchandise,
                ShippingFee = shippingResult.TotalShippingFee,
                OrderVoucherDiscountAmount = orderDiscount,
                ShippingVoucherDiscountAmount = shippingDiscount,
                GrandTotal = Math.Max(
                    0,
                    merchandise + shippingResult.TotalShippingFee -
                    orderDiscount - shippingDiscount),
                OrderVoucher = ToApplied(orderVoucher),
                ShippingVoucher = ToApplied(shippingVoucher),
                AvailableOrderVouchers =
                    await _vouchers.GetAvailableVouchersForContextAsync(
                        userId,
                        context,
                        VoucherType.OrderDiscount,
                        ct),
                AvailableShippingVouchers =
                    await _vouchers.GetAvailableVouchersForContextAsync(
                        userId,
                        context,
                        VoucherType.ShippingDiscount,
                        ct),
                StoreShippingFees = shippingResult.StoreFees
            };
        }

        private static AppliedVoucherDto? ToApplied(VoucherValidationResult? result)
        {
            if (result?.IsValid != true || !result.VoucherId.HasValue)
            {
                return null;
            }

            return new AppliedVoucherDto
            {
                VoucherId = result.VoucherId.Value,
                Code = result.VoucherCode ?? string.Empty,
                Name = result.VoucherName ?? string.Empty,
                Scope = result.Scope,
                StoreId = result.StoreId,
                DiscountAmount = result.DiscountAmount
            };
        }

        private static CheckoutResponseDto Fail(
            string message,
            bool isConflict = false) => new()
        {
            Success = false,
            IsConflict = isConflict,
            ErrorMessage = message
        };

        private static CheckoutResponseDto FromExisting(Order order)
        {
            if (order.StatusHistories.Any(x =>
                    x.NewStatus == OrderStatus.Cancelled ||
                    x.NewStatus == OrderStatus.Rejected ||
                    x.NewStatus == OrderStatus.Refunded))
            {
                return Fail(
                    "This checkout request was already processed and is no longer active. Start a new checkout request.");
            }

            var payment = order.Payments.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            return new CheckoutResponseDto
            {
                Success = true,
                OrderId = order.Id,
                OrderCode = order.OrderCode.ToString(),
                CheckoutUrl = payment?.CheckoutUrl,
                Amount = payment == null ? (long)order.GrandTotal : (long)payment.Amount
            };
        }
    }
}
